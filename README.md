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

- kubernetes (k3d)

    ```shell
    k3d cluster create mycluster -p "8887:80@loadbalancer" --agents 1
    dapr init -k

    # Although the config/secret component of dapr is not used, the component has been loaded in the code, so it must be configured
    # And the code will throw an error: 
    # failed getting secrets from secret store aws-agent-secret: secrets is forbidden:
    # User "system:serviceaccount:default:default" cannot list resource "secrets" in API group "" in the namespace "default"
    # so, add list verbs to role
    kubectl get role secret-reader -n default -o yaml > secret-reader-role.yaml

    # use mongodb cloud atlas, so not deploy it in k8s
    kubectl create secret generic mongodb-state-secret \
        --from-literal=mongodb_state_host=****** \
        --from-literal=mongodb_state_username=****** \
        --from-literal=mongodb_state_password=******

    # aws-agent secret
    kubectl create secret generic aws-agent-secret \
        --from-literal=Mongodb=****** \
        --from-literal=AccessKeyId=****** \
        --from-literal=SecretAccessKey=******

    docker build -f ./src/AwsAgent/Dockerfile -t aws-agent .
    docker build -f ./src/ResourceManager/Dockerfile -t resource-manager .
    k3d image import aws-agent -c mycluster
    k3d image import resource-manager -c mycluster
    kubectl apply -f ./deploy/k8s/ -R

    # There are still problems with traefik configuration, so use port-forward
    kubectl port-forward svc/rabbitmq-ui 8888:15672
    ```

## reference
