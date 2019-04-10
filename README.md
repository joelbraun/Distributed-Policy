## Overview

This repository is an attempt at writing a fully distributed policy service for use in a k8s environment. Policy takes many forms in microservice-based systems, and this particular implementation is designed to be functional across almost all scenarios.

There are two system designs through which policy are usually approached, RBAC and ABAC. RBAC, or Role-Based Access Control, makes authorization decisions based on a user's roles and corresponding permissions. This satisfies a significant number of use cases, and can reasonably handle most policy situations.

ABAC, or Attribute-Based Access Control, is both more flexible and offers finer control. ABAC policy is more dynamic, e.g. "A user can view a document if the document is in the same department as the user" or "Deny access before 9am". This is ideal for more complex authorization scenarios.

ABAC architectures usually contain several components:

- **Policy Enforcement Point (PEP):** The client responsible for governing access to data ABAC has been applied to. Generates an authorization request and then sends it to the PDP.
- **Policy Decision Point (PDP):** Evaluates incoming requests against policy rules, and provides an allow/deny decision.
- **Policy Information Point (PIP):** Provides data necessary to evaluate policies (e.g. a database, LDAP, or Active Directory).  

It's not uncommon, though not required, to see the PDP reach directly out to PIPs to gather information necessary to generate an authorization decision. This project introduces a small twist on this.

## Project Architecture

[Solution Design](https://github.com/joelbraun/Distributed-Policy/raw/master/doc/arch.png)

### Policy Decision Point

The Policy Decision Point is essentially a rules engine. OPA, or the Open Policy Agent, is a great example of a [policy decision engine](https://github.com/open-policy-agent/opa), and was used as the basis for the PDP of this project. There's a lot that OPA can do for you, including Kafka auth, k8s auth, and more that are outside the scope of this. OPA offers a couple key pieces of functionality that make it convenient for distributed systems:

- Since it is almost purely a rules engine, it can be easily deployed alongside containerized apps as a sidecar. This allows policy decisionmaking to occur alongside apps on the same cluster node, with little perceptible round trip time on authorization evaluations.
- OPA can "subscribe" to policy updates. At certain intervals, it can pull policy from a pre-specified source and refresh its policy rules to use during evaluation. This allows policy to be dynamically updated at runtime and propogated to distributed hosts. Naturally, which hosts pull what policy can be segmented as well.

At present, OPA only offers simple support for reaching out directly to PIPs for information. I think there's value in decoupling the permissions data from the policy evaluator anyway, though.

### Policy Information Point

Since OPA solves the difficult part (writing a good rules engine) for us, the real problem to solve is how to get it decision data. There are a few key considerations in propogating highly-distributed policy data:

- A centralized policy data service provides a single point of failure for all your applications. If the system is down, all of your apps will lack the data necessary to make authorization decisions. There are ways around this, like shared caches, putting policy data in your authentication ticket, etc. but these don't really work for all scenarios (lots of permissions? lots of roles? lots of apps with different roles?).
- Any non-caching solution must consider round-trip-time to gather data. HTTP calls to a different service incur significant cost, especially if these calls must occur on every processed request.

Similar to OPA's sidecar architecture, I have implemented a PIP sidecar. This is also deployed alongside client application containers on the same node, to keep call round-trip-time low. This has the added benefit of ensuring applications are not dependent on a single service, but rather are only dependent on their deployed sidecar. This sidecar then pulls from a high-speed, high-availability datastore with sufficient read replication to scale effectively. Your only limitation is the database.

Because each app is deployed with data sidecars, these sidecars can also be tuned for different scenarios. Low number of users, high policy complexity? Cache this app's permission _per user_. High number of users, simple policy? Cache _per role_. Cache in-memory. Have an app with a shared cache if you want. It doesn't matter, because each app has its own sidecar and data access is **isolated**.

**Note:** How you design this datastore is up to you (keys, app namespacing, etc.). All that can vary and is sometimes worthy of case-by-case discussion. Just make it fast!

### Policy Enforcement Point

In this case, I've created a sample .NET app to act as a client in need of authorization. .NET Core now offers [rich support for authorization policy](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2) on its own. This policy integration allows for convenient abstraction of the policy "nuts-and-bolts" of this solution.

My PEP authorization layer (which could easily be abstracted into a library) creates a number of .NET policy attributes. Within these attributes, their actions pull data keyed based on `{app}/{identifier}/{user}` from the PIP sidecar, and then use that data to construct a request to evaluate a policy loaded in the OPA sidecar. OPA's returned decision then acts as the allow/deny response for the .NET `[Authorize]` attribute.

Of course, all this is just .NET Core/MVC syntactic sugar, and similar constructs exist for many other languages.

#### RBAC vs ABAC, and the Pareto Principle

Realistically, 20% of the functionality will get you 80% of the way with policy. Most applications can authorize requests based on simple RBAC roles alone, and for those, we need not involve the OPA sidecar at all. A simple query to the dataservice "Does this user have this role?" or "Does this user have this permission?" is enough to handle the decision right there in the app.

## Project Map

The project is divided into four folders.

- **Client** contains a sample, policy-consuming application.
- **DataService** contains a sample PIP service.
- **Policy** contains a sample OPA rego policy that is dynamically distributed to the OPA sidecar.
- **Kube** contains yaml files that create a k8s deployment with pods that contain the client app, PIP sidecar, and PDP sidecar. It also creates a configmap which holds the OPA policy rules, and a MongoDB service to act as the policy data store.

## Sample Policy Decision Flow

1. A user makes a request to the client application (PEP) to access an endpoint. They have provided a JWT bearer token as "authentication" for this request.
2. The client application (for our purposes, .NET) has an `[Authorize(Policy = "Salary")]` attribute on this endpoint. The .NET Core policy middleware knows to evaluate this request in the context of a "salary" data authorization.
3. The policy middleware requests the `Salary` policy handler and begins execution. This handler parses the claims in the JWT, and requests the permissions from the sidecar PIP for the user based on the app identifier and `sub` (user identifier) claim.
4. The sidecar PIP responds to the .NET Core service with the requested data for the user. For my implementation, this is a collection of permissions based on simple roles.
5. The client application assembles this policy data into a request for the OPA sidecar (PDP) to evaluate a policy. It then calls the OPA sidecar. (Optionally, if all we needed was RBAC information from the store, the authorization can complete in the client.)
6. The OPA sidecar, having pulled the latest policy rules from its remote subscription, evaluates the policy and returns the result as a JSON object.
7. The client application receives this result and allows/denies the request.

## Future Work

Currently, the OPA sidecar pulls policy from a k8s `configmap`. This is sufficient to demonstrate OPA's ability to remotely request policy and update dynamically, but it'd be nice to build some kind of OPA rules hosting service.

You could get creative and reduce sidecars by ensuring that each node scheduled one or two PIP and/or PDP containers for whatever apps were on there, rather than just doing one of each per app container.

The PIP sidecar in this implementation communicates with the .NET app over gRPC. This is pretty fast, and probably good enough for most scenarios (especially since payload sizes are low, and they're guaranteed to be on the same cluster node). But, there are faster ways to cross-communicate between containers, like IPC, that might be cool to try out.  

## Thanks To

[This Wikipedia article](https://en.wikipedia.org/wiki/Attribute-based_access_control) I stole my explanation of ABAC from.

[This wonderful talk](https://www.youtube.com/watch?v=R6tUNpRpdnY) from Netflix where they've implemented someting similar.

## Running It

1. Run `deploySample.sh` in `kube` to deploy to a local minikube cluster.
2. Run `makeRequest.py` to make a sample request to the client application.