![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")

## Team name
⟹ E-Square-V

## Category
⟹ Best addition to the Sitecore MVP Site

## Description
⟹ This work will introduce some additional Foundation Layers to the Sitecore MVP site that connects with Sitecore Experience Edge to fetch the Layout Response data. The Sitecore Experience Edge is a futuristic API based Service from Sitecore and its replicated and scalable. Currently the Sitecore MVP Site does not offer to use Experience Edge with GraphQL queries to fetch the layout route data of the pages.

This project will enable the Sitecore MVP Site to connect with Sitecore Experience Edge with the help of the CustomRenderingEngineMiddleware and CustomGraphQLEdgeConnector layers.


### How does this module solve it ?


The below two additional foundation layers have been introduced to achieve this.

  - CustomRenderingEngineMiddleware
  - CustomGraphQLEdgeConnector

The new Foundation Layers in the Mvp-Site solution as below,

![Solution Update](docs/images/New_Foundation_Layer.config.png?raw=true "Solution Update")


The following diagram summerizes the current behaviour verus the new approach with Sitecore Experience Edge.

### 👉 Current Approach with JSS Sitecore Layout Service


![Existing Approach](docs/images/Current_Working_Behaviour.png?raw=true "Existing Approach")

### 👉 New Approach with Sitecore Experience Edge


![New One](docs/images/New_Approach_Experience_Edge_Endpointss.png?raw=true "New One")

## Video link

🏆 ⟹ ⟹ [Link to YouTube video demo](https://youtu.be/_VRACU4IBjE)

### Presentation

💾 ⟹ ⟹ [Link to the Presentation ](https://github.com/Sitecore-Hackathon/2022-E-Square-V/blob/main/docs/SitecoreHackathon_2022.pptx)



## Pre-requisites and Dependencies

- [.NET Core (>= v 3.1) and .NET Framework 4.8](https://dotnet.microsoft.com/download)
- [MKCert](https://github.com/FiloSottile/mkcert)
- Approx 40gb HD space
- [Okta Developer Account](https://developer.okta.com/signup/)

## 📢 Installation instructions

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
- To Stop the environment again  
   
   ```ps1
   .\down.ps1
   ```  

### Configuration

⟹ No further configuration needed

## 📢 Usage instructions

### To make the RenderingEngineMiddleware to use the CustomHttpRequestHandler

- In the startup.cs file of the RenderingHost, we need to comment out the default request handler and configure our customlayoutrequesthandler like below.

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


## Comments

### ❓ How does it work ?

1. The user visits the Sitecore MVP site
1. The request will be handled by the RenderingEngineMiddleware in the RenderingHost
1. As we have registered the CustomLayoutRequestHandler with the middleware in the Startup.cs, this custom handler will be handling the requests
1. It checks whether the setting **Foundation:Middleware:EndpointConfiguration:UseExperienceEdgeEndpoint** is set to **true**
    1. The CustomLayoutRequestHandler will be using the foundation layer CustomGraphQLEdgeConnector to connect with the Experience Edge endpoint.
    2. The CustomGraphQLEdgeConnector has the below predefined GraphQL query to read the layout response of the given page. It takes site, path and language as the input parameters
    ![GraphQL Query](docs/images/GraphQLQuery.png?raw=true "GraphQL Query")
    3. Once the Query is contructed, it uses the GraphQL.Client to make the GraphQL layout request and fetches the layout response 
    4. Then the response is sent back to the CustomLayoutRequestHandler where it is deserialized into an object of type SitecoreLayoutResponseContent and the SitecoreLayoutResponse is formed
    5. The RenderingEngineMiddleware uses this LayoutResponse to render the page back
1. If the setting **Foundation:Middleware:EndpointConfiguration:UseExperienceEdgeEndpoint** is set to **false**, then its working by default and uses the JSS Layout Services.

This way we can switch between the Experience Edge End point and JSS Headless Service by this setting 'Foundation:Middleware:EndpointConfiguration:UseExperienceEdgeEndpoint'.

