#!/bin/sh
set -e

# Load .env file into environment
if [ -f ".env" ]; then
  export $(grep -v '^#' .env | xargs)
fi

ENVIRONMENT=$1


main() {
  if ! docker network ls | grep -q 'hodhod-backend-app-network'; then
    docker network create hodhod-backend-app-network
  fi

  docker run --rm --network hodhod-backend-app-network --env-file .env ${DC_IMAGE_NAME}-migrator:${DC_IMAGE_TAG}
}

main "$@"
