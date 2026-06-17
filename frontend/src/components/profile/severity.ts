import type { Severity } from "@/lib/types";

type MuiColor = "default" | "warning" | "error" | "success";

/** Maps backend severity to a MUI palette color + a readable hex for borders. */
export function severityColor(severity: Severity): MuiColor {
  switch (severity) {
    case "Red":
      return "error";
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
      return "#c62828";
    case "Yellow":
      return "#c77800";
    case "Green":
      return "#2e7d54";
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
