"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";

export interface LinePoint {
  year: number;
  value: number;
}

export interface LineSeries {
  name: string;
  color: string;
  /** Points incl. the shared branch point so the dashed line connects. */
  points: LinePoint[];
}

interface Props {
  /** Shared measured history, drawn once as a solid line with filled markers. */
  history: LinePoint[];
  historyLabel?: string;
  historyColor?: string;
  /** One dashed projection line per series (do-nothing + scenarios). */
  series: LineSeries[];
  /** Upper bound of the y axis (e.g. 100 for percentages). */
  yMax?: number;
  ySuffix?: string;
  height?: number;
  /** Label drawn above the projected (forecast) span. */
  forecastLabel?: string;
}

/**
 * Comparison line chart: one solid measured "reality so far" line plus a fan of
 * dashed projection lines that branch from its last point. Used to show the
 * do-nothing forecast next to the counterfactual "with our system" scenarios.
 * A self-contained legend is drawn inside the SVG so exported images explain
 * themselves.
 */
export default function MultiLineChart({
  history,
  historyLabel = "Виміряно",
  historyColor = "#25324b",
  series,
  yMax,
  ySuffix = "",
  height = 360,
  forecastLabel = "Прогноз (оцінка)",
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || !containerRef.current || history.length === 0) return;

    const render = () => {
      const width = containerRef.current!.clientWidth;
      const margin = { top: 64, right: 52, bottom: 36, left: 40 };
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

      // union of all years across history + every series
      const years = Array.from(
        new Set([
          ...history.map((d) => d.year),
          ...series.flatMap((s) => s.points.map((p) => p.year)),
        ]),
      ).sort((a, b) => a - b);

      const x = d3
        .scalePoint<number>()
        .domain(years)
        .range([0, innerW])
        .padding(0.5);

      const allValues = [
        ...history.map((d) => d.value),
        ...series.flatMap((s) => s.points.map((p) => p.value)),
      ];
      const maxV = yMax ?? (d3.max(allValues) ?? 0) * 1.1;
      const y = d3.scaleLinear().domain([0, maxV]).range([innerH, 0]);

      // forecast band behind the projected span (from the branch year onward)
      const branchYear = history.at(-1)!.year;
      const branchX = x(branchYear)!;
      g.append("rect")
        .attr("x", branchX)
        .attr("y", 0)
        .attr("width", innerW - branchX)
        .attr("height", innerH)
        .attr("fill", historyColor)
        .attr("opacity", 0.05);
      g.append("text")
        .attr("x", innerW)
        .attr("y", 12)
        .attr("text-anchor", "end")
        .style("font-size", "10px")
        .style("fill", "#7a869a")
        .text(forecastLabel);

      // gridlines + y axis
      g.append("g")
        .call(
          d3
            .axisLeft(y)
            .ticks(5)
            .tickSize(-innerW)
            .tickFormat((d) => `${d}${ySuffix}`),
        )
        .call((sel) => sel.select(".domain").remove())
        .call((sel) => sel.selectAll("line").attr("stroke", "#eef0f6"))
        .selectAll("text")
        .style("font-size", "11px")
        .style("fill", "#7a869a");

      // x axis
      g.append("g")
        .attr("transform", `translate(0,${innerH})`)
        .call(d3.axisBottom(x).tickFormat(d3.format("d")))
        .selectAll("text")
        .style("font-size", "11px");

      const line = d3
        .line<LinePoint>()
        .x((d) => x(d.year)!)
        .y((d) => y(d.value));

      // solid measured history (drawn once)
      g.append("path")
        .datum(history)
        .attr("fill", "none")
        .attr("stroke", historyColor)
        .attr("stroke-width", 2.5)
        .attr("d", line);

      g.selectAll("circle.hist")
        .data(history)
        .join("circle")
        .attr("class", "hist")
        .attr("cx", (d) => x(d.year)!)
        .attr("cy", (d) => y(d.value))
        .attr("r", 4)
        .attr("fill", historyColor)
        .attr("stroke", "#ffffff")
        .attr("stroke-width", 1.5);

      // dashed projection lines + endpoint markers and labels
      series.forEach((s) => {
        g.append("path")
          .datum(s.points)
          .attr("fill", "none")
          .attr("stroke", s.color)
          .attr("stroke-width", 2.5)
          .attr("stroke-dasharray", "6 5")
          .attr("d", line);

        const last = s.points.at(-1)!;
        g.append("circle")
          .attr("cx", x(last.year)!)
          .attr("cy", y(last.value))
          .attr("r", 4)
          .attr("fill", "#ffffff")
          .attr("stroke", s.color)
          .attr("stroke-width", 2);

        g.append("text")
          .attr("x", x(last.year)! + 7)
          .attr("y", y(last.value) + 4)
          .attr("text-anchor", "start")
          .style("font-size", "11px")
          .style("font-weight", "700")
          .style("fill", s.color)
          .text(`${last.value}${ySuffix}`);
      });

      // self-contained legend in the top margin (wraps if needed)
      const legend = g.append("g").attr("transform", "translate(0,-46)");
      const entries = [
        { name: historyLabel, color: historyColor, dashed: false },
        ...series.map((s) => ({ name: s.name, color: s.color, dashed: true })),
      ];
      let lx = 0;
      let ly = 0;
      entries.forEach((e) => {
        const item = legend.append("g");
        item
          .append("line")
          .attr("x1", 0)
          .attr("x2", 22)
          .attr("y1", 0)
          .attr("y2", 0)
          .attr("stroke", e.color)
          .attr("stroke-width", 2.5)
          .attr("stroke-dasharray", e.dashed ? "6 4" : null);
        const txt = item
          .append("text")
          .attr("x", 28)
          .attr("y", 4)
          .style("font-size", "11px")
          .style("fill", "#25324b")
          .text(e.name);
        const w = (txt.node() as SVGTextElement).getComputedTextLength();
        const itemW = 28 + w + 22;
        if (lx + itemW > innerW && lx > 0) {
          lx = 0;
          ly += 18;
        }
        item.attr("transform", `translate(${lx},${ly})`);
        lx += itemW;
      });
    };

    render();
    const ro = new ResizeObserver(render);
    ro.observe(containerRef.current);
    return () => ro.disconnect();
  }, [history, historyLabel, historyColor, series, yMax, ySuffix, height, forecastLabel]);

  return (
    <div ref={containerRef} style={{ width: "100%" }}>
      <svg ref={svgRef} role="img" aria-label="Scenario comparison chart" />
    </div>
  );
}
