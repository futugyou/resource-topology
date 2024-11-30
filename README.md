# ResourceTopology

A project for displaying resource topology

## run

- docker compose

    ```ps
    mkdir -p ./dapr_scheduler/0
    sudo chown -R 1000:1000 ./dapr_scheduler/0
    sudo chmod -R 755 ./dapr_scheduler/0
    docker-compose -f ./docker-compose.dapr.yml up
    ```


- kubernetes

    ```ps
    kubectl create secret generic rabbitmq-secret \
        --from-literal=RABBITMQ_DEFAULT_USER=user \
        --from-literal=RABBITMQ_DEFAULT_PASS=password

    # use mongodb cloud atlas, so not deploy it in k8s
    kubectl create secret generic mongodb-state-secret \
        --from-literal=mongodb_state_host=****** \
        --from-literal=mongodb_state_username=****** \
        --from-literal=mongodb_state_password=******

    kubectl apply -f ./deploy/k8s/ -R
    ```

## reference
