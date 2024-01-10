docker build . --no-cache --file .\src\FeedManager.Silo\Dockerfile -t csrakowski/feedmanagersilo:latest
docker build . --no-cache --file .\src\FeedManager.WebClient\Dockerfile -t csrakowski/feedmanagerwebclient:latest
