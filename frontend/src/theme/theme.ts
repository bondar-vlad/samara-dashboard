"use client";

import { createTheme } from "@mui/material/styles";

/**
 * App theme.
 * - Primary is black, as requested.
 * - Secondary / other accents are nudged off the MUI defaults so the UI doesn't
 *   look like stock Material, while staying close enough to feel familiar.
 */
const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#0a0a0a",
      light: "#2b2b2b",
      dark: "#000000",
      contrastText: "#ffffff",
    },
    secondary: {
      // default MUI secondary is #9c27b0 (purple); shifted to a muted teal-slate
      main: "#3f7d7b",
      light: "#6aa9a6",
      dark: "#2b5654",
      contrastText: "#ffffff",
    },
    // nudged a touch off the defaults
    error: { main: "#c62828" },
    warning: { main: "#c77800" },
    info: { main: "#3b6ea5" },
    success: { main: "#2e7d54" },
    background: {
      default: "#fafafa",
      paper: "#ffffff",
    },
  },
  shape: {
    borderRadius: 10,
  },
  typography: {
    fontFamily: "var(--font-geist-sans), system-ui, Arial, sans-serif",
    h1: { fontWeight: 700 },
    h2: { fontWeight: 700 },
    h3: { fontWeight: 600 },
    button: { textTransform: "none", fontWeight: 600 },
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
    },
    MuiAppBar: {
      defaultProps: { color: "primary" },
    },
  },
});

export default theme;
