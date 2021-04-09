#!/bin/bash

v=$1
image=torbencarstens/templakefrontend
k8sfile=.kubernetes/manifest.yaml

npm run prod
docker build -t $image:$v .
docker push $image:$v
sed -i -e "s/{{tag}}/${v}/g" $k8sfile
kubectl apply -f $k8sfile
sed -i -e "s/${v}/{{tag}}/g" $k8sfile
