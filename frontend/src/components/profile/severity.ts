import type { Severity } from "@/lib/types";
import { RED, YELLOW, ORANGE, GREEN } from "@/theme/colors";

type MuiColor = "default" | "warning" | "error" | "success" | "secondary";

/** Maps backend severity to a MUI palette color + a readable hex for borders. */
export function severityColor(severity: Severity): MuiColor {
  switch (severity) {
    case "Red":
      return "error";
    case "Orange":
      return "secondary";
    case "Yellow":
      return "warning";
    case "Green":
      return "success";
    default:
      return "default";
  }
}

export function severityHex(severity: Severity): string {
  switch (severity) {
    case "Red":
      return RED;
    case "Orange":
      return ORANGE;
    case "Yellow":
      return YELLOW;
    case "Green":
      return GREEN;
    default:
      return "#9e9e9e";
  }
}

/** Human-readable Ukrainian labels for the audiences enum. */
const AUDIENCE_UK: Record<string, string> = {
  ClassTeacher: "Класний керівник",
  Student: "Учень",
  Parent: "Батьки",
  SchoolAdmin: "Адміністрація",
  Psychologist: "Психолог",
  SocialWorker: "Соцпрацівник",
};

export function audienceLabel(a: string): string {
  return AUDIENCE_UK[a] ?? a;
}
