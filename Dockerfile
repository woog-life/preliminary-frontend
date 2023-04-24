FROM crystallang/crystal:1.8.1-alpine AS builder

RUN apk update && \
    apk add ca-certificates && \
    update-ca-certificates

WORKDIR /usr/app

COPY shard.yml shard.lock ./

RUN shards install --no-color --production

COPY public/ public/
COPY src/ src/

RUN shards build --static --no-debug --release --production --warnings=all

FROM alpine:3.17.3

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
    # uid
    -u 1001 \
    # do not create a home directory
    -H \
    launcher

RUN chown -R launcher:launcher /usr/app
RUN chmod -R o+w /usr/app

USER 1001

ENV KEMAL_ENV production

ENTRYPOINT ["/usr/app/frontend"]
