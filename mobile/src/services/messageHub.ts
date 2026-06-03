import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';
import { API_BASE_URL } from '../config/api';
import { MessageDto } from '../types/message';

declare const require: (moduleName: string) => any;

type MessageHandler = (message: MessageDto) => void;

class MessageHubService {
  private connection: any = null;
  private isConnecting = false;
  private handlers = new Set<MessageHandler>();

  private async getAccessToken(): Promise<string | null> {
    if (Platform.OS === 'web') {
      return localStorage.getItem('hirenix_access_token');
    }
    return SecureStore.getItemAsync('hirenix_access_token');
  }

  private emitMessage(message: MessageDto) {
    this.handlers.forEach((handler) => {
      try {
        handler(message);
      } catch (error) {
        console.error('[Hirenix][messageHub] handler error', error);
      }
    });
  }

  private getSignalRModule(): any | null {
    try {
      return require('@microsoft/signalr');
    } catch {
      if (__DEV__) {
        console.warn(
          '[Hirenix][messageHub] @microsoft/signalr is not installed. Realtime messaging is disabled.',
        );
      }
      return null;
    }
  }

  async ensureConnected(): Promise<boolean> {
    if (this.connection?.state === 'Connected') {
      return true;
    }

    if (this.isConnecting) {
      return false;
    }

    const signalR = this.getSignalRModule();
    if (!signalR) {
      return false;
    }

    const token = await this.getAccessToken();
    if (!token) {
      return false;
    }

    this.isConnecting = true;
    try {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/hubs/messages`, {
          accessTokenFactory: async () => {
            const nextToken = await this.getAccessToken();
            return nextToken || '';
          },
        })
        .withAutomaticReconnect()
        .build();

      connection.on('MessageReceived', (message: MessageDto) => {
        this.emitMessage(message);
      });

      connection.onclose(() => {
        this.connection = null;
      });

      await connection.start();
      this.connection = connection;
      return true;
    } catch (error) {
      console.error('[Hirenix][messageHub] connection failed', error);
      return false;
    } finally {
      this.isConnecting = false;
    }
  }

  subscribe(handler: MessageHandler): () => void {
    this.handlers.add(handler);
    return () => {
      this.handlers.delete(handler);
    };
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    try {
      await this.connection.stop();
    } catch (error) {
      console.error('[Hirenix][messageHub] disconnect failed', error);
    } finally {
      this.connection = null;
    }
  }
}

export const messageHubService = new MessageHubService();
