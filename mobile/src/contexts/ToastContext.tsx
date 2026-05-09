/**
 * ToastContext — Hirenix
 * Global toast management với queue system
 */
import React, { createContext, useContext, useState, useCallback } from 'react';
import { Toast, ToastType } from '../components/Toast';

interface ToastContextType {
  showToast: (message: string, type?: ToastType, duration?: number) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

interface ToastItem {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [currentToast, setCurrentToast] = useState<ToastItem | null>(null);
  const [queue, setQueue] = useState<ToastItem[]>([]);

  const showToast = useCallback((message: string, type: ToastType = 'info', duration?: number) => {
    console.log('🔔 showToast called:', { message, type, duration });
    const newToast: ToastItem = {
      id: Date.now().toString(),
      message,
      type,
      duration,
    };

    if (!currentToast) {
      console.log('✅ Setting current toast');
      setCurrentToast(newToast);
    } else {
      console.log('📋 Adding to queue');
      setQueue((prev) => [...prev, newToast]);
    }
  }, [currentToast]);

  const handleDismiss = useCallback(() => {
    setCurrentToast(null);
    
    // Show next toast in queue
    setQueue((prev) => {
      if (prev.length > 0) {
        const [next, ...rest] = prev;
        setCurrentToast(next);
        return rest;
      }
      return prev;
    });
  }, []);

  console.log('🎨 ToastProvider render, currentToast:', currentToast);

  return (
    <>
      <ToastContext.Provider value={{ showToast }}>
        {children}
      </ToastContext.Provider>
      {currentToast && (
        <Toast
          message={currentToast.message}
          type={currentToast.type}
          visible={!!currentToast}
          onDismiss={handleDismiss}
          duration={currentToast.duration}
        />
      )}
    </>
  );
};

export const useToast = (): ToastContextType => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within ToastProvider');
  }
  return context;
};
