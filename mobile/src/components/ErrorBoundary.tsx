import React from 'react';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { BorderRadius, Colors, Spacing, Typography } from '../constants/theme';

type ErrorBoundaryState = {
  hasError: boolean;
  message: string;
};

type ErrorBoundaryProps = {
  children: React.ReactNode;
};

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      message: '',
    };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return {
      hasError: true,
      message: error?.message || 'Unexpected error',
    };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('[Hirenix][ErrorBoundary]', error, errorInfo?.componentStack);
  }

  private handleRetry = () => {
    this.setState({ hasError: false, message: '' });
  };

  render() {
    if (!this.state.hasError) {
      return this.props.children;
    }

    return (
      <View style={styles.container}>
        <View style={styles.card}>
          <Text style={styles.title}>Oops! Có lỗi xảy ra</Text>
          <Text style={styles.message}>{this.state.message || 'Ứng dụng gặp lỗi không mong muốn.'}</Text>

          <TouchableOpacity style={styles.button} onPress={this.handleRetry} activeOpacity={0.85}>
            <Text style={styles.buttonText}>Thử lại</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.gray50,
    padding: Spacing.base,
  },
  card: {
    width: '100%',
    borderRadius: BorderRadius.lg,
    backgroundColor: Colors.white,
    borderWidth: 1,
    borderColor: Colors.gray200,
    padding: Spacing.lg,
  },
  title: {
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    marginBottom: Spacing.sm,
  },
  message: {
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    lineHeight: Typography.lineHeight.bodyMd,
    marginBottom: Spacing.base,
  },
  button: {
    height: 46,
    borderRadius: BorderRadius.md,
    backgroundColor: Colors.primaryBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  buttonText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.bodyMd,
  },
});
