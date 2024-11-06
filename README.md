# ResourceTopology

A project for displaying resource topology

## run

- env

    ```ps
    export ACCESS_KEY_ID=xxxx
    export SECRET_ACCESS_KEY=xxxx
    export AWSAGENT_MONGODB_URL=xxxx
    ```

- docker

    ```ps
    docker build -f src/ResourceManager/Dockerfile .
    docker build -f src/AwsAgent/Dockerfile .
    ```

- docker compose

    ```ps
    docker-compose -f ./docker-compose.debug.yml up
    ```

- dapr

    ```ps
    dapr run -f ./dapr
    ```

## reference
