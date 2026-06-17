/**
 * Export an inline <svg> chart to a downloadable image.
 * Supports SVG (vector), PNG and JPEG (raster, rendered via canvas at 2x).
 */
export type ExportFormat = "svg" | "png" | "jpeg";

const FONT_STACK = "system-ui, -apple-system, Segoe UI, Arial, sans-serif";

function serializeSvg(svg: SVGSVGElement): {
  text: string;
  width: number;
  height: number;
} {
  const clone = svg.cloneNode(true) as SVGSVGElement;

  // Width/height may be set as attributes by D3; fall back to the bounding box.
  const rect = svg.getBoundingClientRect();
  const width = Number(svg.getAttribute("width")) || rect.width || 800;
  const height = Number(svg.getAttribute("height")) || rect.height || 400;

  clone.setAttribute("xmlns", "http://www.w3.org/2000/svg");
  clone.setAttribute("width", String(width));
  clone.setAttribute("height", String(height));
  clone.setAttribute("viewBox", `0 0 ${width} ${height}`);

  // Inline a font + a white background so the standalone file looks right.
  const style = document.createElementNS("http://www.w3.org/2000/svg", "style");
  style.textContent = `text { font-family: ${FONT_STACK}; }`;
  clone.insertBefore(style, clone.firstChild);

  const bg = document.createElementNS("http://www.w3.org/2000/svg", "rect");
  bg.setAttribute("x", "0");
  bg.setAttribute("y", "0");
  bg.setAttribute("width", String(width));
  bg.setAttribute("height", String(height));
  bg.setAttribute("fill", "#ffffff");
  clone.insertBefore(bg, style.nextSibling);

  const text = new XMLSerializer().serializeToString(clone);
  return { text, width, height };
}

function triggerDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

export async function exportChart(
  svg: SVGSVGElement,
  filename: string,
  format: ExportFormat,
): Promise<void> {
  const { text, width, height } = serializeSvg(svg);

  if (format === "svg") {
    triggerDownload(
      new Blob([text], { type: "image/svg+xml;charset=utf-8" }),
      `${filename}.svg`,
    );
    return;
  }

  // Raster: draw the SVG onto a 2x canvas, then export.
  const scale = 2;
  const svgUrl =
    "data:image/svg+xml;charset=utf-8," + encodeURIComponent(text);

  await new Promise<void>((resolve, reject) => {
    const img = new Image();
    img.onload = () => {
      const canvas = document.createElement("canvas");
      canvas.width = width * scale;
      canvas.height = height * scale;
      const ctx = canvas.getContext("2d");
      if (!ctx) return reject(new Error("Canvas 2D context unavailable"));
      ctx.fillStyle = "#ffffff";
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      const mime = format === "png" ? "image/png" : "image/jpeg";
      canvas.toBlob(
        (blob) => {
          if (!blob) return reject(new Error("Failed to encode image"));
          triggerDownload(blob, `${filename}.${format}`);
          resolve();
        },
        mime,
        0.95,
      );
    };
    img.onerror = () => reject(new Error("Failed to load SVG for rasterizing"));
    img.src = svgUrl;
  });
}
