export interface ApiConfig {
    baseUrl: string
}

export interface ObservabilityConfig {
    connectionString: string
}

export interface AppConfig {
    api: ApiConfig
    observability: ObservabilityConfig
}

const config: AppConfig = {
    api: {
        baseUrl: window.ENV_CONFIG.TODO_REACT_REST_API_BASE_URL || 'http://localhost:3100'
    },
    observability: {
        connectionString: window.ENV_CONFIG.TODO_REACT_REST_APPLICATIONINSIGHTS_CONNECTION_STRING || ''
    }
}

export default config;