# Firepuma.Payments

## Introduction

This solution was generated with [francoishill/Firepuma.Template.GoogleCloudRunService](https://github.com/francoishill/Firepuma.Template.GoogleCloudRunService).

The following projects were generated as part of the solution:

* Firepuma.Payments.Domain project contains the domain logic (not tightly coupled to Mongo or other infrastructure specifics)
* Firepuma.Payments.Infrastructure contains infrastructure code, like mongo repositories inheriting from `MongoDbRepository<T>`
* Firepuma.Payments.Tests contains unit tests
* Firepuma.Payments.Worker project contains the service that will get deployed to Google Cloud Run

---

## Deploying

When using github, the deployment will happen automatically due to the folder containing workflow yaml files in the `.github/workflows` folder.

To test locally whether the Dockerfile can build, run the following command:

```shell
docker build --tag tmp-test-firepuma-payments-webapi --progress plain --file Firepuma.Payments.WebApi/Dockerfile --build-arg version=0.0.0-dev-from-readme .
&& docker run --rm --name tmp-webapi tmp-test-firepuma-payments-webapi
```

```shell
docker build --tag tmp-test-firepuma-payments-worker --progress plain --file Firepuma.Payments.Worker/Dockerfile --build-arg version=0.0.0-dev-from-readme .
&& docker run --rm --name tmp-webapi tmp-test-firepuma-payments-worker
```