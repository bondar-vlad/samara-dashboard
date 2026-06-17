"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";

export interface DonutSlice {
  label: string;
  value: number;
  color: string;
}

interface Props {
  data: DonutSlice[];
  height?: number;
  /** Text shown in the center (e.g. a headline number). */
  centerLabel?: string;
  centerSub?: string;
}

/** Donut chart in D3/SVG with a center label and a legend. */
export default function DonutChart({
  data,
  height = 320,
  centerLabel,
  centerSub,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !containerRef.current || data.length === 0) return;

    const render = () => {
      const width = containerRef.current!.clientWidth;
      const svg = d3
        .select(svgRef.current)
        .attr("width", width)
        .attr("height", height);
      svg.selectAll("*").remove();

      const radius = Math.min(width, height) / 2 - 8;
      const cx = width / 2;
      const cy = height / 2;
      const g = svg.append("g").attr("transform", `translate(${cx},${cy})`);

      const pie = d3
        .pie<DonutSlice>()
        .value((d) => d.value)
        .sort(null);
      const arc = d3
        .arc<d3.PieArcDatum<DonutSlice>>()
        .innerRadius(radius * 0.62)
        .outerRadius(radius);

      const arcs = pie(data);

      g.selectAll("path")
        .data(arcs)
        .join("path")
        .attr("fill", (d) => d.data.color)
        .attr("d", arc as never)
        .attr("stroke", "#fff")
        .attr("stroke-width", 2);

      // percentage labels on slices
      g.selectAll("text.slice")
        .data(arcs)
        .join("text")
        .attr("class", "slice")
        .attr("transform", (d) => `translate(${arc.centroid(d)})`)
        .attr("text-anchor", "middle")
        .attr("dy", "0.35em")
        .style("font-size", "12px")
        .style("font-weight", "700")
        .style("fill", "#fff")
        .text((d) => `${d.data.value}%`);

      if (centerLabel) {
        g.append("text")
          .attr("text-anchor", "middle")
          .attr("dy", centerSub ? "-0.1em" : "0.35em")
          .style("font-size", "26px")
          .style("font-weight", "700")
          .style("fill", "#25324b")
          .text(centerLabel);
      }
      if (centerSub) {
        g.append("text")
          .attr("text-anchor", "middle")
          .attr("dy", "1.4em")
          .style("font-size", "12px")
          .style("fill", "#7a869a")
          .text(centerSub);
      }
    };

    render();
    const ro = new ResizeObserver(render);
    ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [data, height, centerLabel, centerSub]);

  return (
    <div ref={containerRef} style={{ width: "100%" }}>
      <svg ref={svgRef} role="img" aria-label="Donut chart" />
    </div>
  );
}
