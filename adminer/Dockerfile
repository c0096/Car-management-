FROM adminer:latest
USER root
RUN apk add --no-cache freetds freetds-dev && docker-php-ext-install pdo_dblib
USER adminer
