FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

ARG GITHUBSHA OPENWEATHERMAP_APIKEY_RAW 

WORKDIR /prelim-frontend

ENV NODE_OPTIONS="--openssl-legacy-provider"

RUN apt update -y && curl -sL https://deb.nodesource.com/setup_17.x | bash -

RUN apt-get install -y nodejs

COPY . .

RUN sed -i -e "s#{{TAG}}#$GITHUBSHA#g" src/App.fs && sed -i -e "s/|> Program.withConsoleTrace//g" src/App.fs && sed -i -e "s#{{API_KEY}}#$OPENWEATHERMAP_APIKEY_RAW#g" src/App.fs

RUN npm install && npm run prod


# Second stage
FROM nginx:1.21-alpine

WORKDIR /preliminary-frontend

COPY --from=builder /prelim-frontend/public/ /usr/share/nginx/html

EXPOSE 80

