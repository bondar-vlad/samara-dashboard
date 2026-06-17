"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";

export interface GroupedDatum {
  label: string;
  /** key -> value for each series in the group */
  values: Record<string, number>;
}

export interface SeriesDef {
  key: string;
  name: string;
  color: string;
}

interface Props {
  data: GroupedDatum[];
  series: SeriesDef[];
  height?: number;
  /** Called when a bar is clicked, with the group label and series key. */
  onBarClick?: (label: string, seriesKey: string) => void;
}

/**
 * Responsive grouped (clustered) bar chart in D3/SVG. Used on the dashboard to
 * compare, per profile cluster, the count recommended by the system vs. the
 * count desired by students.
 */
export default function GroupedBarChart({
  data,
  series,
  height = 340,
  onBarClick,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !containerRef.current) return;

    const render = () => {
      const width = containerRef.current!.clientWidth;
      const margin = { top: 16, right: 16, bottom: 84, left: 36 };
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

      const x0 = d3
        .scaleBand<string>()
        .domain(data.map((d) => d.label))
        .range([0, innerW])
        .paddingInner(0.25)
        .paddingOuter(0.1);

      const x1 = d3
        .scaleBand<string>()
        .domain(series.map((s) => s.key))
        .range([0, x0.bandwidth()])
        .padding(0.1);

      const maxVal =
        d3.max(data, (d) => d3.max(series, (s) => d.values[s.key] ?? 0)) ?? 0;

      const y = d3
        .scaleLinear()
        .domain([0, Math.max(1, maxVal)])
        .nice()
        .range([innerH, 0]);

      // y axis (integer ticks)
      g.append("g")
        .call(d3.axisLeft(y).ticks(Math.min(5, Math.max(1, maxVal))).tickFormat(d3.format("d")))
        .selectAll("text")
        .style("font-size", "11px");

      // x axis with wrapped/rotated labels
      const xAxis = g
        .append("g")
        .attr("transform", `translate(0,${innerH})`)
        .call(d3.axisBottom(x0));
      xAxis
        .selectAll("text")
        .style("font-size", "11px")
        .attr("transform", "rotate(-18)")
        .style("text-anchor", "end")
        .each(function () {
          const self = d3.select(this);
          const text = self.text();
          if (text.length > 22) self.text(text.slice(0, 21) + "…");
        });

      // groups
      const group = g
        .selectAll("g.group")
        .data(data)
        .join("g")
        .attr("class", "group")
        .attr("transform", (d) => `translate(${x0(d.label)},0)`);

      group
        .selectAll("rect")
        .data((d) =>
          series.map((s) => ({
            key: s.key,
            color: s.color,
            value: d.values[s.key] ?? 0,
            label: d.label,
          })),
        )
        .join("rect")
        .attr("x", (d) => x1(d.key) ?? 0)
        .attr("width", x1.bandwidth())
        .attr("y", innerH)
        .attr("height", 0)
        .attr("rx", 3)
        .attr("fill", (d) => d.color)
        .style("cursor", onBarClick ? "pointer" : "default")
        .on("click", (_e, d) => onBarClick?.(d.label, d.key))
        .transition()
        .duration(550)
        .attr("y", (d) => y(d.value))
        .attr("height", (d) => innerH - y(d.value));

      // value labels on top of bars
      group
        .selectAll("text.val")
        .data((d) =>
          series.map((s) => ({
            key: s.key,
            value: d.values[s.key] ?? 0,
          })),
        )
        .join("text")
        .attr("class", "val")
        .attr("x", (d) => (x1(d.key) ?? 0) + x1.bandwidth() / 2)
        .attr("y", (d) => y(d.value) - 4)
        .attr("text-anchor", "middle")
        .style("font-size", "11px")
        .style("font-weight", "600")
        .style("opacity", 0)
        .text((d) => (d.value > 0 ? d.value : ""))
        .transition()
        .delay(400)
        .duration(300)
        .style("opacity", 1);
    };

    render();
    const ro = new ResizeObserver(render);
    ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [data, series, height, onBarClick]);

  return (
    <div ref={containerRef} style={{ width: "100%" }}>
      <svg ref={svgRef} role="img" aria-label="Grouped bar chart" />
    </div>
  );
}
