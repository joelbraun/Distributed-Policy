#!/bin/bash

# Bundle up the policies
tar -zcvf bundle.tar.gz policies

# Send them to the cluster
kubectl create secret generic policybundle --from-file=bundle.tar.gz