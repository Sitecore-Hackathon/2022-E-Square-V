![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")

## Team name
⟹ E-Square-V

## Category
⟹ Best addition to the Sitecore MVP Site

## Description
⟹ The Sitecore MVP site an Open Source project and it is based on the Sitecore Headless Rendering SDK and uses Sitecore Layout Service; a headless API end point that provides Sitecore page route data as JSON response. Then this JSON response is used by the default LayoutRequestHandler to render the pages. 

The Sitecore Experience Edge is a futuristic API based Service from Sitecore and its replicated and scalable. Currently the Sitecore MVP Site does not offer to use Experience Edge with GraphQL queries to fetch the layout route data of the pages.

If we scaffold the Sitecore JSS app, by default it provides both 

### What was problem solved ?


This project will enable the Sitecore MVP Site to connect with Sitecore Experience Edge by means of the CustomRenderingEngineMiddleware and CustomGraphQLEdgeConnector.


### How does this module solve it ?


The below two additional foundation layers have been introduced to achieve this.

  - CustomRenderingEngineMiddleware
  - CustomGraphQLEdgeConnector

The new Foundation Layers in the Mvp-Site solution as below,

![Solution Update](docs/images/New_Foundation_Layers.config.png?raw=true "Solution Update")


The following diagram summerizes the current behaviour verus the new approach with Sitecore Experience Edge.

### Current Approach with JSS Sitecore Layout Service


![Existing Approach](docs/images/Current_Working_Behaviour.png?raw=true "Existing Approach")

### New Approach with Sitecore Experience Edge


![New One](docs/images/New_Approach_Experience_Edge_Endpoint.png?raw=true "New One")

## Video link

⟹ [Replace this Video link](#video-link)



## Pre-requisites and Dependencies

- [.NET Core (>= v 3.1) and .NET Framework 4.8](https://dotnet.microsoft.com/download)
- [MKCert](https://github.com/FiloSottile/mkcert)
- Approx 40gb HD space
- [Okta Developer Account](https://developer.okta.com/signup/)

## Installation instructions

### Note

  - Always use an elevated/Administrator Windows Powershell 5.1 to run the commands

### Steps

- Please make sure the IIS is stopped
- Run the below Powershell command to prepare the environment. It will take care of adding host entries and creates wildcard certificates

```ps1
    .\init.ps1 -InitEnv -LicenseXmlPath "[Path to the license folder]\license.xml" -AdminPassword "DesiredAdminPassword"
``` 
- It would have added the below host entries,
     * mvp-cd.sc.localhost
     * mvp-cm.sc.localhost
     * mvp-id.sc.localhost
     * mvp.sc.localhost
- In the `.env` the Okta developer account details must be populated. 
   - OKTA_DOMAIN (*must* include protocol, e.g. OKTA_DOMAIN=**https**://dev-your-id.okta.com)
   - OKTA_CLIENT_ID
   - OKTA_CLIENT_SECRET
- After completing this environment preparation, run the startup script
   from the solution root:
    ```ps1
    .\up.ps1
    ```
- It will prompt for the login to sitecore. Login and accept the device verification
- Wait for the script to complete and open the CM, CD and rendering host sites

### Configuration

⟹ No further configuration needed

## Usage instructions

As stated early, This approach uses two layers,

1. CustomRenderingMiddleware - This exposes a CustomLayoutRequestHandler to be configured in the startup.cs
1. GraphQLEdgeConnector - This layer will make the GraphQLLayoutRequest to the Experience Edge end point to fetch the page route data.


### To make the RenderingEngineMiddleware to use the CustomHttpRequestHandler

- In the startup.cs file of the RenderingHost, we need to comment out the default request handler and configure our customrequesthandler like below.

![Startup configuration](docs/images/Startup_Configuration.png?raw=true "Startup configuration")

### App Setting Configuration in the Rendering Host

- In the rendering host appsettings.config file, you need add the below configuration changes for the CustomRenderingEngine Middleware to use Sitecore Experience Edge and for the GraphQLEdgeConnector

```json
  "Foundation": {
    "GraphQLEdgeConnector": {
      "GraphQL": {
        "UrlLive": "http://cm/sitecore/api/graph/edge",
        "UrlEdit": "http://cm/sitecore/api/graph/edge",
        "BypassRemoteCertificateValidation": true
      }
    },
    "Middleware": {
      "EndpointConfiguration": {
        "UseExperienceEdgeEndpoint": "true"
      }
    }
  }
```
- When the setting **Foundation:Middleware:EndpointConfiguration:UseExperienceEdgeEndpoint** is set to **true**, the CustomLayoutRequestHandler uses the Sitecore Experience Edge. 
- When the same is set to **false**, it uses default JSS headless service

## Comments
If you'd like to make additional comments that is important for your module entry.
