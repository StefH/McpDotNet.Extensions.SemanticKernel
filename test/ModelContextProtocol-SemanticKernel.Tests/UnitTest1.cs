using FluentAssertions;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Newtonsoft.Json.Linq;
using WireMock.Matchers;
using WireMock.Server;

namespace ModelContextProtocol.SemanticKernel.Tests;

public sealed class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        await using var tuple = await GetMcpClientAsync();
        var mcpClient = tuple.Item1;

        // Assert
        var tools = await tuple.Item1.ListToolsAsync();
        tools.Should().HaveCount(17);

        var commits = await mcpClient.CallToolAsync("list_commits", new Dictionary<string, object?> { { "owner", "StefH" }, { "repo", "FluentBuilder" } });
        commits.Content.SelectMany(c => c.Text ?? string.Empty).Should().Contain("229388090f50a39f489e30cb535f67f3705cf61f");
    }

    private static async Task<AsyncDisposableTuple<IMcpClient, WireMockServer>> GetMcpClientAsync(CancellationToken cancellationToken = default)
    {
        var server = InitWireMockServer();

        McpClientOptions options = new()
        {
            ClientInfo = new() { Name = "GitHub Test", Version = "1.0.0" }
        };

        var config = new McpServerConfig
        {
            Id = "github",
            Name = "GitHub",
            TransportType = TransportTypes.Sse,
            Location = server.Url
        };

        var client = await McpClientFactory.CreateAsync(config, options, null, LoggerFactory.Create(c => c.AddConsole()), cancellationToken);

        return new(client, server);
    }

    private static WireMockServer InitWireMockServer()
    {
        var tscTools = new TaskCompletionSource<string>();
        var tscListCommits = new TaskCompletionSource<string>();

        var server = WireMockServer.Start();
        server
            .WhenRequest(r =>
                r.UsingGet()
            )
            .ThenRespondWith(r => r
                .WithHeader("Content-Type", "text/event-stream")
                .WithHeader("Cache-Control", "no-cache")
                .WithHeader("Connection", "keep-alive")
                .WithSseBody(async (_, q) =>
                {
                    q.Write($"event: endpoint\r\ndata: {server.Url}/sse\r\n\r\n");

                    var toolsResponse = await tscTools.Task;
                    q.Write($"event: message\r\ndata: {toolsResponse}\r\n\r\n");

                    var commitsResponse = await tscListCommits.Task;
                    q.Write($"event: message\r\ndata: {commitsResponse}\r\n\r\n");

                    q.Close();
                })
            );

        server
            .WhenRequest(r => r
                .UsingPost()
                .WithPath("/sse")
                .WithHeader("Content-Type", "application/json*")
                .WithBody(new JsonPartialWildcardMatcher(new { method = "initialize" }))
            )
            .ThenRespondWith(r => r
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                          {"jsonrpc":"2.0","id":"{{request.bodyAsJson.id}}","result":{"protocolVersion":"2024-11-05","capabilities":{"logging":{},"prompts":{"listChanged":true},"resources":{"subscribe":true,"listChanged":true},"tools":{"listChanged":true}},"serverInfo":{"name":"ExampleServer","version":"1.0.0"}}}
                          """)
                .WithTransformer()
            );

        server
            .WhenRequest(r => r
                .UsingPost()
                .WithPath("/sse")
                .WithHeader("Content-Type", "application/json*")
                .WithBody(b => b?.Contains("{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}") == true)
            )
            .ThenRespondWith(r => r
                .WithBody("accepted")
            );

        server
            .WhenRequest(r => r
                .UsingPost()
                .WithPath("/sse")
                .WithHeader("Content-Type", "application/json*")
                .WithBody(new JsonPartialWildcardMatcher(new { method = "tools/list" }))
            )
            .ThenRespondWith(r => r
                .WithBody(req =>
                {
                    const string response =
                        """
                        {"result":{"tools":[{"name":"create_or_update_file","description":"Create or update a single file in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"path":{"type":"string","description":"Path where to create/update the file"},"content":{"type":"string","description":"Content of the file"},"message":{"type":"string","description":"Commit message"},"branch":{"type":"string","description":"Branch to create/update the file in"},"sha":{"type":"string","description":"SHA of the file being replaced (required when updating existing files)"}},"required":["owner","repo","path","content","message","branch"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"search_repositories","description":"Search for GitHub repositories","inputSchema":{"type":"object","properties":{"query":{"type":"string","description":"Search query (see GitHub search syntax)"},"page":{"type":"number","description":"Page number for pagination (default: 1)"},"perPage":{"type":"number","description":"Number of results per page (default: 30, max: 100)"}},"required":["query"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"create_repository","description":"Create a new GitHub repository in your account","inputSchema":{"type":"object","properties":{"name":{"type":"string","description":"Repository name"},"description":{"type":"string","description":"Repository description"},"private":{"type":"boolean","description":"Whether the repository should be private"},"autoInit":{"type":"boolean","description":"Initialize with README.md"}},"required":["name"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"get_file_contents","description":"Get the contents of a file or directory from a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"path":{"type":"string","description":"Path to the file or directory"},"branch":{"type":"string","description":"Branch to get contents from"}},"required":["owner","repo","path"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"push_files","description":"Push multiple files to a GitHub repository in a single commit","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"branch":{"type":"string","description":"Branch to push to (e.g., 'main' or 'master')"},"files":{"type":"array","items":{"type":"object","properties":{"path":{"type":"string"},"content":{"type":"string"}},"required":["path","content"],"additionalProperties":false},"description":"Array of files to push"},"message":{"type":"string","description":"Commit message"}},"required":["owner","repo","branch","files","message"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"create_issue","description":"Create a new issue in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"title":{"type":"string"},"body":{"type":"string"},"assignees":{"type":"array","items":{"type":"string"}},"milestone":{"type":"number"},"labels":{"type":"array","items":{"type":"string"}}},"required":["owner","repo","title"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"create_pull_request","description":"Create a new pull request in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"title":{"type":"string","description":"Pull request title"},"body":{"type":"string","description":"Pull request body/description"},"head":{"type":"string","description":"The name of the branch where your changes are implemented"},"base":{"type":"string","description":"The name of the branch you want the changes pulled into"},"draft":{"type":"boolean","description":"Whether to create the pull request as a draft"},"maintainer_can_modify":{"type":"boolean","description":"Whether maintainers can modify the pull request"}},"required":["owner","repo","title","head","base"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"fork_repository","description":"Fork a GitHub repository to your account or specified organization","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"organization":{"type":"string","description":"Optional: organization to fork to (defaults to your personal account)"}},"required":["owner","repo"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"create_branch","description":"Create a new branch in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string","description":"Repository owner (username or organization)"},"repo":{"type":"string","description":"Repository name"},"branch":{"type":"string","description":"Name for the new branch"},"from_branch":{"type":"string","description":"Optional: source branch to create from (defaults to the repository's default branch)"}},"required":["owner","repo","branch"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"list_commits","description":"Get list of commits of a branch in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"sha":{"type":"string"},"page":{"type":"number"},"perPage":{"type":"number"}},"required":["owner","repo"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"list_issues","description":"List issues in a GitHub repository with filtering options","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"direction":{"type":"string","enum":["asc","desc"]},"labels":{"type":"array","items":{"type":"string"}},"page":{"type":"number"},"per_page":{"type":"number"},"since":{"type":"string"},"sort":{"type":"string","enum":["created","updated","comments"]},"state":{"type":"string","enum":["open","closed","all"]}},"required":["owner","repo"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"update_issue","description":"Update an existing issue in a GitHub repository","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"issue_number":{"type":"number"},"title":{"type":"string"},"body":{"type":"string"},"assignees":{"type":"array","items":{"type":"string"}},"milestone":{"type":"number"},"labels":{"type":"array","items":{"type":"string"}},"state":{"type":"string","enum":["open","closed"]}},"required":["owner","repo","issue_number"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"add_issue_comment","description":"Add a comment to an existing issue","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"issue_number":{"type":"number"},"body":{"type":"string"}},"required":["owner","repo","issue_number","body"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"search_code","description":"Search for code across GitHub repositories","inputSchema":{"type":"object","properties":{"q":{"type":"string"},"order":{"type":"string","enum":["asc","desc"]},"page":{"type":"number","minimum":1},"per_page":{"type":"number","minimum":1,"maximum":100}},"required":["q"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"search_issues","description":"Search for issues and pull requests across GitHub repositories","inputSchema":{"type":"object","properties":{"q":{"type":"string"},"order":{"type":"string","enum":["asc","desc"]},"page":{"type":"number","minimum":1},"per_page":{"type":"number","minimum":1,"maximum":100},"sort":{"type":"string","enum":["comments","reactions","reactions-+1","reactions--1","reactions-smile","reactions-thinking_face","reactions-heart","reactions-tada","interactions","created","updated"]}},"required":["q"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"search_users","description":"Search for users on GitHub","inputSchema":{"type":"object","properties":{"q":{"type":"string"},"order":{"type":"string","enum":["asc","desc"]},"page":{"type":"number","minimum":1},"per_page":{"type":"number","minimum":1,"maximum":100},"sort":{"type":"string","enum":["followers","repositories","joined"]}},"required":["q"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}},{"name":"get_issue","description":"Get details of a specific issue in a GitHub repository.","inputSchema":{"type":"object","properties":{"owner":{"type":"string"},"repo":{"type":"string"},"issue_number":{"type":"number"}},"required":["owner","repo","issue_number"],"additionalProperties":false,"$schema":"http://json-schema.org/draft-07/schema#"}}]},"jsonrpc":"2.0","id":"$ID$"}
                        """;
                    tscTools.SetResult(response.Replace("$ID$", ((JObject)req.BodyAsJson!)["id"]!.Value<string>()));
                    return "accepted";
                })
            );

        server
            .WhenRequest(r => r
                .UsingPost()
                .WithPath("/sse")
                .WithBody(new JsonPartialWildcardMatcher(new { method = "tools/call" }))
            )
            .ThenRespondWith(r => r
                .WithBody(req =>
                {
                    const string response =
                        """
                        {"result":{"content":[{"type":"text","text":"[\n  {\n    \"sha\": \"229388090f50a39f489e30cb535f67f3705cf61f\",\n    \"node_id\": \"C_kwDOFxUkCdoAKDIyOTM4ODA5MGY1MGEzOWY0ODllMzBjYjUzNWY2N2YzNzA1Y2Y2MWY\",\n    \"commit\": {\n      \"author\": {\n        \"name\": \"Stef Heyenrath\",\n        \"email\": \"Stef.Heyenrath@gmail.com\",\n        \"date\": \"2025-01-30T19:02:23Z\"\n      },\n      \"committer\": {\n        \"name\": \"GitHub\",\n        \"email\": \"noreply@github.com\",\n        \"date\": \"2025-01-30T19:02:23Z\"\n      },\n      \"message\": \"Update README.md\",\n      \"tree\": {\n        \"sha\": \"2e563e6934950afccf66ee7b67b5c7d5262a0e7c\",\n        \"url\": \"https://api.github.com/repos/StefH/FluentBuilder/git/trees/2e563e6934950afccf66ee7b67b5c7d5262a0e7c\"\n      },\n      \"url\": \"https://api.github.com/repos/StefH/FluentBuilder/git/commits/229388090f50a39f489e30cb535f67f3705cf61f\",\n      \"comment_count\": 0,\n      \"verification\": {\n        \"verified\": true,\n        \"reason\": \"valid\",\n        \"signature\": \"-----BEGIN PGP SIGNATURE-----\n\nwsFcBAABCAAQBQJnm8y/CRC1aQ7uu5UhlAAAcTcQABkVyZ3vDgmEkhq7ylmRCBd5\n2AvH6kJIXo7N+Egjeh75yGM6USzaMlKJZgFWVKuIiyjIFAHpOKivPzuVEs+Y/Y2I\ncrJQ1rSSOSCl2uvMDS/Fe/L4fonsdh81ukwthB4+6pqfjfgGNK7myDypUKVeDo/g\nZncP9vl5R1btIWwzhBqPArG5rJIddXmFaUCj+NveMy58nK7xZROSVONrp2CsA0gK\nAaK7bziZSTs2m54Gm79K078fSIPcMx3tRbSk/8o8GInUyO8iVXaw/ZYB4dyptPOy\nY3cJTEsi04tuF7pm3IXleC7nWYE18qQm9m1uoaSLQimt7K8IYxGDRi8grhTWVERE\njArI8oQ7u5NK5SQHCsXH0wiF3IeDEdaYWoBsRq+czoEgS647pOkBd4Ca29dkTs6+\nqrVphULqnX9eJuwSfjdfhNOyaNlKDNx7WYSqN8c+IN+K6BoFpaTx9xFb787XcdGQ\n1QfKlq6nTZtSdsOrS1GxFEaq48V0HayKaHDIaxxuKyGwzIFn5k5O43Y6ku1kWVCy\nBMUO9b0sY59zHoGqqdBwt3WgZLdx03eXQ+eu0cT52rfy4H5rhbqILLXYGoLlhB7A\n1MYtuCiz0G4QXPZuFBMYpSEM2ry8N6OjYUQGXgdvom4nx03C5PfgVqYDcX9PZDSO\naEw4P4zVVRY4OFYgMm53\n=1FOD\n-----END PGP SIGNATURE-----\n\",\n        \"payload\": \"tree 2e563e6934950afccf66ee7b67b5c7d5262a0e7c\nparent ae2706424c3b75613bf5625091aa2649fb33ecde\nauthor Stef Heyenrath <Stef.Heyenrath@gmail.com> 1738263743 +0100\ncommitter GitHub <noreply@github.com> 1738263743 +0100\n\nUpdate README.md\",\n        \"verified_at\": \"2025-01-30T19:07:26Z\"\n      }\n    },\n    \"url\": \"https://api.github.com/repos/StefH/FluentBuilder/commits/229388090f50a39f489e30cb535f67f3705cf61f\",\n    \"html_url\": \"https://github.com/StefH/FluentBuilder/commit/229388090f50a39f489e30cb535f67f3705cf61f\",\n    \"comments_url\": \"https://api.github.com/repos/StefH/FluentBuilder/commits/229388090f50a39f489e30cb535f67f3705cf61f/comments\",\n    \"author\": {\n      \"login\": \"StefH\",\n      \"id\": 249938,\n      \"node_id\": \"MDQ6VXNlcjI0OTkzOA==\",\n      \"avatar_url\": \"https://avatars.githubusercontent.com/u/249938?v=4\",\n      \"gravatar_id\": \"\",\n      \"url\": \"https://api.github.com/users/StefH\",\n      \"html_url\": \"https://github.com/StefH\",\n      \"followers_url\": \"https://api.github.com/users/StefH/followers\",\n      \"following_url\": \"https://api.github.com/users/StefH/following{/other_user}\",\n      \"gists_url\": \"https://api.github.com/users/StefH/gists{/gist_id}\",\n      \"starred_url\": \"https://api.github.com/users/StefH/starred{/owner}{/repo}\",\n      \"subscriptions_url\": \"https://api.github.com/users/StefH/subscriptions\",\n      \"organizations_url\": \"https://api.github.com/users/StefH/orgs\",\n      \"repos_url\": \"https://api.github.com/users/StefH/repos\",\n      \"events_url\": \"https://api.github.com/users/StefH/events{/privacy}\",\n      \"received_events_url\": \"https://api.github.com/users/StefH/received_events\",\n      \"type\": \"User\",\n      \"user_view_type\": \"public\",\n      \"site_admin\": false\n    },\n    \"committer\": {\n      \"login\": \"web-flow\",\n      \"id\": 19864447,\n      \"node_id\": \"MDQ6VXNlcjE5ODY0NDQ3\",\n      \"avatar_url\": \"https://avatars.githubusercontent.com/u/19864447?v=4\",\n      \"gravatar_id\": \"\",\n      \"url\": \"https://api.github.com/users/web-flow\",\n      \"html_url\": \"https://github.com/web-flow\",\n      \"followers_url\": \"https://api.github.com/users/web-flow/followers\",\n      \"following_url\": \"https://api.github.com/users/web-flow/following{/other_user}\",\n      \"gists_url\": \"https://api.github.com/users/web-flow/gists{/gist_id}\",\n      \"starred_url\": \"https://api.github.com/users/web-flow/starred{/owner}{/repo}\",\n      \"subscriptions_url\": \"https://api.github.com/users/web-flow/subscriptions\",\n      \"organizations_url\": \"https://api.github.com/users/web-flow/orgs\",\n      \"repos_url\": \"https://api.github.com/users/web-flow/repos\",\n      \"events_url\": \"https://api.github.com/users/web-flow/events{/privacy}\",\n      \"received_events_url\": \"https://api.github.com/users/web-flow/received_events\",\n      \"type\": \"User\",\n      \"user_view_type\": \"public\",\n      \"site_admin\": false\n    },\n    \"parents\": [\n      {\n        \"sha\": \"ae2706424c3b75613bf5625091aa2649fb33ecde\",\n        \"url\": \"https://api.github.com/repos/StefH/FluentBuilder/commits/ae2706424c3b75613bf5625091aa2649fb33ecde\",\n        \"html_url\": \"https://github.com/StefH/FluentBuilder/commit/ae2706424c3b75613bf5625091aa2649fb33ecde\"\n      }\n    ]\n  }\n]"}]},"jsonrpc":"2.0","id":"$ID$"}
                        """;
                    tscListCommits.SetResult(response.Replace("$ID$", ((JObject)req.BodyAsJson!)["id"]!.Value<string>()));
                    return "accepted";
                })
            );

        return server;
    }
}