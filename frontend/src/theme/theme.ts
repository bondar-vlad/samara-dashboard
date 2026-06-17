"use client";

import { createTheme } from "@mui/material/styles";
import {
  BLUE,
  BLUE_LIGHT,
  BLUE_DARK,
  ORANGE,
  ORANGE_LIGHT,
  ORANGE_DARK,
  YELLOW,
  GREEN,
  RED,
  TEXT_PRIMARY,
  TEXT_SECONDARY,
  BG_DEFAULT,
  BORDER,
  CARD_SHADOW,
} from "./colors";

/**
 * App theme — blue/orange "Rocket"-style dashboard look:
 * vivid royal-blue primary, orange secondary, soft-shadowed rounded white
 * cards on a light cool-gray background, pill-shaped buttons.
 */
const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: BLUE,
      light: BLUE_LIGHT,
      dark: BLUE_DARK,
      contrastText: "#ffffff",
    },
    secondary: {
      main: ORANGE,
      light: ORANGE_LIGHT,
      dark: ORANGE_DARK,
      contrastText: "#ffffff",
    },
    warning: { main: YELLOW, contrastText: "#ffffff" },
    success: { main: GREEN, contrastText: "#ffffff" },
    error: { main: RED, contrastText: "#ffffff" },
    info: { main: "#4dabf7", contrastText: "#ffffff" },
    background: {
      default: BG_DEFAULT,
      paper: "#ffffff",
    },
    text: {
      primary: TEXT_PRIMARY,
      secondary: TEXT_SECONDARY,
    },
    divider: BORDER,
  },
  shape: {
    borderRadius: 14,
  },
  typography: {
    fontFamily: "var(--font-geist-sans), system-ui, Arial, sans-serif",
    h1: { fontWeight: 700 },
    h2: { fontWeight: 700 },
    h3: { fontWeight: 700 },
    h4: { fontWeight: 700 },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 700 },
    button: { textTransform: "none", fontWeight: 600 },
  },
  components: {
    MuiCard: {
      defaultProps: { elevation: 0 },
      styleOverrides: {
        root: {
          borderRadius: 16,
          border: `1px solid ${BORDER}`,
          boxShadow: CARD_SHADOW,
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        outlined: { borderColor: BORDER },
      },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: { borderRadius: 999, paddingInline: 18 },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { borderRadius: 8, fontWeight: 600 },
      },
    },
    MuiAppBar: {
      defaultProps: { color: "primary", elevation: 0 },
      styleOverrides: {
        root: { boxShadow: "0 2px 16px rgba(58, 87, 232, 0.18)" },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        head: {
          color: TEXT_SECONDARY,
          fontWeight: 700,
          fontSize: "0.75rem",
          textTransform: "uppercase",
          letterSpacing: "0.04em",
        },
      },
    },
    MuiLinearProgress: {
      styleOverrides: {
        root: { borderRadius: 999 },
      },
    },
  },
});

export default theme;
