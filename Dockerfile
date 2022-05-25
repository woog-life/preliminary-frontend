FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

ARG GITHUBSHA

ARG OPENWEATHERMAP_APIKEY_RAW=""

WORKDIR /app

# This is due to the openssl 3.0 change
# Changes can be found here: https://nodejs.org/en/blog/release/v17.0.0/
# https://www.openssl.org/blog/blog/2021/09/07/OpenSSL3.Final/
ENV NODE_OPTIONS="--openssl-legacy-provider"

RUN apt-get update -y && curl -sL https://deb.nodesource.com/setup_17.x | bash -

RUN apt-get install -y nodejs

COPY . .

RUN sed -i -e "s#{{TAG}}#$GITHUBSHA#g" src/App.fs && \
    sed -i -e "s/|> Program.withConsoleTrace//g" src/App.fs && \
    sed -i -e "s#{{API_KEY}}#$OPENWEATHERMAP_APIKEY_RAW#g" src/App.fs

RUN npm install \
    && npm run prod


FROM nginx:1.22-alpine

COPY --from=builder /app/public/ /usr/share/nginx/html

EXPOSE 80
