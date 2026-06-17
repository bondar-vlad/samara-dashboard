"use client";

import { useId } from "react";
import Select, {
  type Props as SelectProps,
  type GroupBase,
  type StylesConfig,
} from "react-select";

export interface Option {
  value: string;
  label: string;
}

/**
 * Dropdown built on react-select (the React-native equivalent of select2 —
 * select2 is a jQuery plugin and doesn't play well with React/Next SSR).
 *
 * Styled to match the app's black primary theme. `instanceId` is derived from
 * useId() to keep SSR and client markup in sync (avoids hydration warnings).
 */
const BLACK = "#0a0a0a";
const BORDER = "#cfcfcf";

function blackStyles<
  IsMulti extends boolean = false,
  Group extends GroupBase<Option> = GroupBase<Option>,
>(): StylesConfig<Option, IsMulti, Group> {
  return {
    control: (base, state) => ({
      ...base,
      minHeight: 40,
      borderRadius: 10,
      borderColor: state.isFocused ? BLACK : BORDER,
      boxShadow: state.isFocused ? `0 0 0 1px ${BLACK}` : "none",
      "&:hover": { borderColor: BLACK },
    }),
    option: (base, state) => ({
      ...base,
      backgroundColor: state.isSelected
        ? BLACK
        : state.isFocused
          ? "#f0f0f0"
          : "#fff",
      color: state.isSelected ? "#fff" : BLACK,
      cursor: "pointer",
      "&:active": { backgroundColor: state.isSelected ? BLACK : "#e4e4e4" },
    }),
    multiValue: (base) => ({
      ...base,
      backgroundColor: BLACK,
      borderRadius: 6,
    }),
    multiValueLabel: (base) => ({ ...base, color: "#fff" }),
    multiValueRemove: (base) => ({
      ...base,
      color: "#fff",
      "&:hover": { backgroundColor: "#2b2b2b", color: "#fff" },
    }),
    dropdownIndicator: (base, state) => ({
      ...base,
      color: state.isFocused ? BLACK : "#777",
      "&:hover": { color: BLACK },
    }),
    menu: (base) => ({ ...base, borderRadius: 10, overflow: "hidden", zIndex: 20 }),
  };
}

export default function BlackSelect<
  IsMulti extends boolean = false,
  Group extends GroupBase<Option> = GroupBase<Option>,
>(props: SelectProps<Option, IsMulti, Group>) {
  const generatedId = useId();
  return (
    <Select<Option, IsMulti, Group>
      instanceId={props.instanceId ?? generatedId}
      styles={blackStyles<IsMulti, Group>()}
      theme={(t) => ({
        ...t,
        colors: { ...t.colors, primary: BLACK, primary25: "#f0f0f0", primary50: "#e4e4e4" },
      })}
      {...props}
    />
  );
}
