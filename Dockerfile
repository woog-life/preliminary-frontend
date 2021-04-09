FROM nginx:1.19-alpine
EXPOSE 80

ADD public /usr/share/nginx/html
