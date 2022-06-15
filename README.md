# Firepuma-Payments-MicroService

A repository containing code for a payments micro service.

[![Deploy function app](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app.yml/badge.svg)](https://github.com/francoishill/Firepuma-Payments-MicroService/actions/workflows/deploy-function-app.yml)

## The plan

The plan of building this repo contained in these steps, not necessarily in this order:

* [ ] Scaffold code with a single payment gateway (PayFast)
* [ ] Deploy with Github Actions
* [ ] Test APIs flow manually (use a dummy app with PayFast sandbox config)
* [ ] Deploy a client library to Nuget
* [ ] Unit tests
* [ ] Create functions to manage third party applications (add the application config and create service bus queues)
* [ ] Add a sample application that uses the Client library
* [ ] Add ability to extend it with other payment gateways