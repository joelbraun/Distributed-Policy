#!/bin/bash

eval $(minikube docker-env)

### policy creation

# Bundle up the policies
tar -zcvf bundle.tar.gz ../policy/policies
# Send them to the cluster
kubectl create secret generic policybundle --from-file=bundle.tar.gz

### image build

# build data service
docker build -t policydataservice:latest ../dataservice
# build client
docker build -t policyclient:latest ../client

### deploy 

# delete the old one, if it exists
kubectl delete deployment policytest-deployment

# rollout mongo, client, OPA, and data service
kubectl apply --recursive -f . --validate=false

### convenience

# print the URL of the service
minikube service policytest-deployment --url
