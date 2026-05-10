/**
 * Google OAuth Configuration for Hirenix
 */

export const GOOGLE_CONFIG = {
  // Web Client ID from Google Cloud Console
  webClientId: '606977292204-3daq46625fbb1dsdtvtkbfk6tqov9guj.apps.googleusercontent.com',
  
  // Android Client ID (optional - add when you create Android OAuth client)
  androidClientId: '',
  
  // iOS Client ID (optional - add when you create iOS OAuth client)
  iosClientId: '',
  
  // Scopes to request from Google
  scopes: ['profile', 'email'],
};

// OAuth endpoints
export const GOOGLE_OAUTH_ENDPOINTS = {
  authorizationEndpoint: 'https://accounts.google.com/o/oauth2/v2/auth',
  tokenEndpoint: 'https://oauth2.googleapis.com/token',
  revocationEndpoint: 'https://oauth2.googleapis.com/revoke',
};
