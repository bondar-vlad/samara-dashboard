"use client";

import { useRef, useState, type ReactNode } from "react";
import {
  Card,
  CardHeader,
  CardContent,
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Tooltip,
  Snackbar,
  Alert,
} from "@mui/material";
import DownloadIcon from "@mui/icons-material/Download";
import ImageIcon from "@mui/icons-material/Image";
import { exportChart, type ExportFormat } from "@/lib/exportChart";

const FORMATS: { format: ExportFormat; label: string }[] = [
  { format: "png", label: "PNG (растрове зображення)" },
  { format: "jpeg", label: "JPEG (растрове зображення)" },
  { format: "svg", label: "SVG (векторне зображення)" },
];

/**
 * Card wrapper for a chart with a built-in "save as image" menu. It exports the
 * first <svg> rendered inside it to PNG / JPEG / SVG.
 */
export default function ChartFrame({
  title,
  subheader,
  filename,
  children,
}: {
  title: string;
  subheader?: string;
  /** Base filename for the downloaded image (no extension). */
  filename: string;
  children: ReactNode;
}) {
  const bodyRef = useRef<HTMLDivElement>(null);
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);
  const [error, setError] = useState<string | null>(null);

  const handleExport = async (format: ExportFormat) => {
    setAnchor(null);
    const svg = bodyRef.current?.querySelector("svg");
    if (!svg) {
      setError("Не вдалося знайти діаграму для експорту.");
      return;
    }
    try {
      await exportChart(svg as SVGSVGElement, filename, format);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Помилка експорту");
    }
  };

  return (
    <Card variant="outlined">
      <CardHeader
        title={title}
        subheader={subheader}
        action={
          <Tooltip title="Зберегти як зображення">
            <IconButton onClick={(e) => setAnchor(e.currentTarget)} aria-label="export chart">
              <DownloadIcon />
            </IconButton>
          </Tooltip>
        }
      />
      <CardContent>
        <div ref={bodyRef}>{children}</div>
      </CardContent>

      <Menu anchorEl={anchor} open={!!anchor} onClose={() => setAnchor(null)}>
        {FORMATS.map((f) => (
          <MenuItem key={f.format} onClick={() => handleExport(f.format)}>
            <ListItemIcon>
              <ImageIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{f.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>

      <Snackbar
        open={!!error}
        autoHideDuration={4000}
        onClose={() => setError(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      </Snackbar>
    </Card>
  );
}
