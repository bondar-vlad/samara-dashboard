"use client";

import { useState, type ReactNode } from "react";
import { ThemeProvider, CssBaseline } from "@mui/material";
import {
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import theme from "@/theme/theme";
import { I18nProvider } from "@/i18n/I18nProvider";
import { RoleProvider } from "@/access/RoleProvider";

/**
 * Client-side providers: TanStack Query (the current name for react-query) +
 * MUI theme. The QueryClient is created in state so it's stable per-render
 * and not shared across requests on the server.
 */
export default function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <I18nProvider>
          <RoleProvider>{children}</RoleProvider>
        </I18nProvider>
      </ThemeProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
