FROM crystallang/crystal:1.6.1-alpine AS builder

RUN apk update && \
    apk add ca-certificates && \
    update-ca-certificates

WORKDIR /usr/app

COPY shard.yml shard.lock ./

RUN shards install --no-color --production

COPY public/ public/
COPY src/ src/

RUN shards build --static --no-debug --release --production --warnings=all
#RUN ldd ./bin/frontend | tr -s '[:blank:]' '\n' | grep '^/' | \
#    xargs -I % sh -c 'mkdir -p $(dirname deps%); cp % deps%;' \

FROM alpine:3.16.2

RUN apk update && \
    apk add --update tzdata && \
    rm -rf /var/cache/apk/*

WORKDIR /usr/app

COPY --from=builder /usr/app/bin/frontend ./frontend
COPY --from=builder /usr/app/public ./public

RUN adduser \
    # don't assign a password \
    -D \
    # shell
    -s /bin/false \
    # gecos
    -g "" \
    # uid
    -u 1001 \
    # do not create a home directory
    -H \
    launcher

RUN chown -R launcher:launcher /usr/app
RUN chmod -R o+w /usr/app

ENTRYPOINT ["/usr/app/frontend"]
