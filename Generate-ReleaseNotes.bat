rem https://github.com/StefH/GitHubReleaseNotes

SET version=0.0.1-preview-05

GitHubReleaseNotes --output ReleaseNotes.md --skip-empty-releases --exclude-labels question invalid doc --version %version% --token %GH_TOKEN%

GitHubReleaseNotes --output PackageReleaseNotes.txt --skip-empty-releases --exclude-labels question invalid doc --template PackageReleaseNotes.template --version %version% --token %GH_TOKEN%