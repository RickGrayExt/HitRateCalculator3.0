FROM node:18 AS build
WORKDIR /app
COPY . .
RUN mkdir -p /dist && cp -r . /dist
FROM nginx:alpine
COPY --from=build /dist /usr/share/nginx/html
EXPOSE 80
