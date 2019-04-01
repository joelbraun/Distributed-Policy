#!/bin/bash

eval $(minikube docker-env)

# build data service
docker build -t policydataservice:latest ../dataservice

# build client
docker build -t policyclient:latest ../client

# rollout mongo, client, OPA, and data service
kubectl apply --recursive -f . --validate=false
