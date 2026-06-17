"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";

export interface BarDatum {
  label: string;
  value: number;
}

interface BarChartProps {
  data: BarDatum[];
  height?: number;
  /** Bar fill — defaults to the app's black primary. */
  color?: string;
}

/**
 * Responsive D3 bar chart rendered to SVG.
 *
 * D3 (SVG) chosen over raw canvas: SVG is easier to make interactive and
 * accessible for dashboard-scale data, and D3 is the most popular free option.
 * Switch to canvas only if we ever push tens of thousands of points.
 */
export default function BarChart({
  data,
  height = 280,
  color = "#0a0a0a",
}: BarChartProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !containerRef.current) return;

    const render = () => {
      const width = containerRef.current!.clientWidth;
      const margin = { top: 16, right: 16, bottom: 36, left: 40 };
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

      const x = d3
        .scaleBand<string>()
        .domain(data.map((d) => d.label))
        .range([0, innerW])
        .padding(0.25);

      const y = d3
        .scaleLinear()
        .domain([0, d3.max(data, (d) => d.value) ?? 0])
        .nice()
        .range([innerH, 0]);

      // axes
      g.append("g")
        .attr("transform", `translate(0,${innerH})`)
        .call(d3.axisBottom(x))
        .selectAll("text")
        .style("font-size", "11px");

      g.append("g")
        .call(d3.axisLeft(y).ticks(5))
        .selectAll("text")
        .style("font-size", "11px");

      // bars with a simple grow-in transition
      g.selectAll("rect")
        .data(data)
        .join("rect")
        .attr("x", (d) => x(d.label) ?? 0)
        .attr("width", x.bandwidth())
        .attr("y", innerH)
        .attr("height", 0)
        .attr("rx", 4)
        .attr("fill", color)
        .transition()
        .duration(600)
        .attr("y", (d) => y(d.value))
        .attr("height", (d) => innerH - y(d.value));
    };

    render();

    const ro = new ResizeObserver(render);
    ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [data, height, color]);

  return (
    <div ref={containerRef} style={{ width: "100%" }}>
      <svg ref={svgRef} role="img" aria-label="Bar chart" />
    </div>
  );
}
