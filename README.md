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
    k3d cluster create mycluster -p "8887:80@loadbalancer" --agents 1 --api-port 0.0.0.0:33801
    dapr init -k

    # Although the config/secret component of dapr is not used, the component has been loaded in the code, so it must be configured
    # And the code will throw an error: 
    # failed getting secrets from secret store aws-agent-secret: secrets is forbidden:
    # User "system:serviceaccount:default:default" cannot list resource "secrets" in API group "" in the namespace "default"
    # so, add list verbs to role
    # kubectl get role secret-reader -n default -o yaml > secret-reader-role.yaml

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

- dapr cmd

    ```ps
    dapr init

    // use env secret
    export AWS_AGENT_ServiceOption__AccessKeyId=
    export AWS_AGENT_ServiceOption__SecretAccessKey=
    export AWS_AGENT_ConnectionStrings__Mongodb=""

    dapr run -f ./deploy/local # This cmd runs quite slowly.
    
    dapr run --app-id aws-agent --dapr-grpc-port 50001 --dapr-http-port 3500 --resources-path ./deploy/local/components -- dotnet run --project ./src/AwsAgent/
    dapr run --app-id resource-manager --app-port 5000 --dapr-grpc-port 50002 --dapr-http-port 3501 \
        --resources-path ./deploy/local/components -- dotnet run --urls=http://localhost:5000/ --project ./src/ResourceManager/

    dapr uninstall
    ```

## reference
