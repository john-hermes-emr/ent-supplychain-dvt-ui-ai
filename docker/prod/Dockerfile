FROM node:20-alpine AS builder

#RUN apt-get update -y && \
#    apt-get install -y yarnpkg

WORKDIR /usr/src/app

COPY package*.json /usr/src/app/

ARG ISSUER
ENV ISSUER=$ISSUER

ARG CLIENT_ID
ENV CLIENT_ID=$CLIENT_ID

ARG REACT_APP_DVT_API_ROOT_PATH
ENV REACT_APP_DVT_API_ROOT_PATH=$REACT_APP_DVT_API_ROOT_PATH

RUN npm install
COPY . /usr/src/app
RUN npm run build

FROM nginx:alpine
COPY docker/prod/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /usr/src/app/dist /usr/share/nginx/html
COPY --from=builder /usr/src/app/asset /usr/share/nginx/html/asset
RUN touch /var/run/nginx.pid
RUN chown -R nginx:nginx /var/run/nginx.pid /usr/share/nginx/html /var/cache/nginx /var/log/nginx /etc/nginx/conf.d
# USER nginx
EXPOSE 80 
CMD ["nginx", "-g", "daemon off;"]