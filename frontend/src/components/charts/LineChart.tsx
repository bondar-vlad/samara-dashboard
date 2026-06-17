"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";

export interface LinePoint {
  year: number;
  value: number;
  kind: "measured" | "estimated";
}

interface Props {
  data: LinePoint[];
  color?: string;
  /** Upper bound of the y axis (e.g. 100 for percentages). */
  yMax?: number;
  ySuffix?: string;
  height?: number;
}

/**
 * Time-series line chart in D3/SVG. The measured span is drawn solid with
 * filled markers; the projected (estimated) span continues as a dashed line
 * with hollow markers, behind a lightly shaded "forecast" band.
 */
export default function LineChart({
  data,
  color = "#3a57e8",
  yMax,
  ySuffix = "",
  height = 320,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !containerRef.current || data.length === 0) return;

    const render = () => {
      const width = containerRef.current!.clientWidth;
      const margin = { top: 24, right: 20, bottom: 36, left: 40 };
      const innerW = Math.max(0, width - margin.left - margin.right);
      const innerH = height - margin.top - margin.bottom;

      const svg = d3
        .select(svgRef.current)
        .attr("width", width)
        .attr("height", height);
      svg.selectAll("*").remove();
      const g = svg
        .append("g")
        .attr("transform", `translate(${margin.left},${margin.top})`);

      const years = data.map((d) => d.year);
      const x = d3
        .scalePoint<number>()
        .domain(years)
        .range([0, innerW])
        .padding(0.5);
      const maxV = yMax ?? (d3.max(data, (d) => d.value) ?? 0) * 1.1;
      const y = d3.scaleLinear().domain([0, maxV]).range([innerH, 0]);

      const lastMeasuredIdx = data.reduce(
        (acc, d, i) => (d.kind === "measured" ? i : acc),
        0,
      );

      // forecast band behind the predicted span
      if (lastMeasuredIdx < data.length - 1) {
        const x0 = x(data[lastMeasuredIdx].year)!;
        g.append("rect")
          .attr("x", x0)
          .attr("y", 0)
          .attr("width", innerW - x0)
          .attr("height", innerH)
          .attr("fill", color)
          .attr("opacity", 0.06);
        g.append("text")
          .attr("x", innerW)
          .attr("y", 12)
          .attr("text-anchor", "end")
          .style("font-size", "10px")
          .style("fill", "#7a869a")
          .text("AI-прогноз");
      }

      // gridlines
      g.append("g")
        .call(d3.axisLeft(y).ticks(5).tickSize(-innerW).tickFormat((d) => `${d}${ySuffix}`))
        .call((sel) => sel.select(".domain").remove())
        .call((sel) => sel.selectAll("line").attr("stroke", "#eef0f6"))
        .selectAll("text")
        .style("font-size", "11px")
        .style("fill", "#7a869a");

      g.append("g")
        .attr("transform", `translate(0,${innerH})`)
        .call(d3.axisBottom(x).tickFormat(d3.format("d")))
        .selectAll("text")
        .style("font-size", "11px");

      const line = d3
        .line<LinePoint>()
        .x((d) => x(d.year)!)
        .y((d) => y(d.value));

      // solid measured segment
      const measured = data.slice(0, lastMeasuredIdx + 1);
      g.append("path")
        .datum(measured)
        .attr("fill", "none")
        .attr("stroke", color)
        .attr("stroke-width", 2.5)
        .attr("d", line);

      // dashed predicted segment (starts at the last measured point)
      const predicted = data.slice(lastMeasuredIdx);
      if (predicted.length > 1) {
        g.append("path")
          .datum(predicted)
          .attr("fill", "none")
          .attr("stroke", color)
          .attr("stroke-width", 2.5)
          .attr("stroke-dasharray", "6 5")
          .attr("d", line);
      }

      // markers + labels
      const pts = g
        .selectAll("g.pt")
        .data(data)
        .join("g")
        .attr("class", "pt")
        .attr("transform", (d) => `translate(${x(d.year)},${y(d.value)})`);

      pts
        .append("circle")
        .attr("r", 5)
        .attr("fill", (d) => (d.kind === "measured" ? color : "#ffffff"))
        .attr("stroke", color)
        .attr("stroke-width", 2);

      pts
        .append("text")
        .attr("y", -10)
        .attr("text-anchor", "middle")
        .style("font-size", "11px")
        .style("font-weight", "600")
        .style("fill", "#25324b")
        .text((d) => `${d.value}${ySuffix}`);
    };

    render();
    const ro = new ResizeObserver(render);
    ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [data, color, yMax, ySuffix, height]);

  return (
    <div ref={containerRef} style={{ width: "100%" }}>
      <svg ref={svgRef} role="img" aria-label="Line chart" />
    </div>
  );
}
