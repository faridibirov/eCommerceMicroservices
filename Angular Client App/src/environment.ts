// src/environments/environment.ts
// export const environment = {
//   production: false,
//   usersAPIURL:
//     'https://farid-ecommerce-api-service.azure-api.net/gateway/users/',
//   productsAPIURL:
//     'https://farid-ecommerce-api-service.azure-api.net/gateway/products/',
//   ordersAPIURL:
//     'https://farid-ecommerce-api-service.azure-api.net/gateway/orders/',
//   gatewayURL: 'https://farid-ecommerce-api-service.azure-api.net/gateway/',
// };


// production: false,
//   usersAPIURL: 'http://localhost:4000/gateway/users/',
//   productsAPIURL: 'http://localhost:4000/gateway/products/',
//   ordersAPIURL: 'http://localhost:4000/gateway/orders/',
//   gatewayURL: 'http://localhost:4000/gateway/',


// src/environments/environment.ts
export const b2c = {
  tenantName: 'faridecommerce',
  tenantId: '75d57e46-6039-4a7b-aca1-5b846d3a8562',
  clientId: '18d922ae-f6ab-4bab-8eef-89fb26a2bcf5',
  apiInstanceName: 'farid-ecommerce-api-service'
};

const serverUrl = `https://${b2c.apiInstanceName}.azure-api.net/`;

export const environment = {
  production: false,
  usersAPIURL: `${serverUrl}gateway/users/`,
  productsAPIURL: `${serverUrl}gateway/products/`,
  ordersAPIURL: `${serverUrl}gateway/orders/`,
  gatewayURL: `${serverUrl}gateway/`,

  config: {
    env_name: 'dev',
    production: true,
    apiUrl: serverUrl,
    apiEndpoints: {
      userProfile:'user-profiles'
    },
    adb2cConfig: {
      clientId: b2c.clientId,
      authority: `https://${b2c.tenantName}.ciamlogin.com/${b2c.tenantId}`,
      authorityDomain: `${b2c.tenantName}.ciamlogin.com`,
      scopeUrls: [
         'https://faridecommerce.onmicrosoft.com/ecommerce/access_as_user'
      ],
      apiEndpointUrls: [ 
        `${serverUrl}gateway/users`, 
        `${serverUrl}gateway/orders`,
        `${serverUrl}gateway/products`
      ],
    },
    cacheTimeInMinutes: 30,
  }
};
