import os
import sys

import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import numpy
import pandas
from matplotlib.gridspec import GridSpec

import plot_common
from GenerateTaskTimeStacked import ROW_DEFS as TIME_ROW_DEFS


def pandas_numeric(series):
    return pandas.to_numeric(series, errors="coerce")


def assign_endpoint_offsets(cluster, offsets):
    if len(cluster) < 2:
        return
    midpoint = (len(cluster) - 1) / 2.0
    for index, endpoint in enumerate(cluster):
        offsets[endpoint[0]] = (index - midpoint) * 18


ROW_NAMES = ["Bottom-Up", "Top-Down", "Other Search"]
COLUMN_NAMES = ["PSMs", "Time", "Peptides / Proteoforms", "Protein Groups"]

# BOXES[row][col] = list of (label, csv column, color-dict key)
BOXES = [
    # Bottom-Up
    [
        [
            ("Initial", "Initial Search PSMs", "Search"),
            ("Post-Calib", "Post-calibration PSMs", "Calibrate"),
            ("Post-GPTMD", "Post-GPTMD PSMs", "GPTMD"),
        ],
        TIME_ROW_DEFS[0][1],
        [
            ("Initial", "Initial Search Peptides", "Search"),
            ("Post-Calib", "Post-calibration Peptides", "Calibrate"),
            ("Post-GPTMD", "Post-GPTMD Peptides", "GPTMD"),
        ],
        [
            ("Initial", "InitialSearchProteinGroups", "Search"),
            ("Post-Calib", "PostCalibrationProteinGroups", "Calibrate"),
            ("Post-GPTMD", "PostGptmdProteinGroups", "GPTMD"),
        ],
    ],
    # Top-Down
    [
        [
            ("Initial", "TopDown Initial PrSMs", "TDSearch"),
            ("Post-Calib", "TopDown Post-calibration PrSMs", "TDCalibrate"),
            ("Post-Avg", "TopDown Post-averaging PrSMs", "TDAverage"),
            ("Post-GPTMD", "TopDown Post-GPTMD PrSMs", "TDGPTMD"),
        ],
        TIME_ROW_DEFS[1][1],
        [
            ("Initial", "TopDown Initial Proteoforms", "TDSearch"),
            (
                "Post-Calib",
                "TopDown Post-calibration Proteoforms",
                "TDCalibrate",
            ),
            ("Post-Avg", "TopDown Post-averaging Proteoforms", "TDAverage"),
            ("Post-GPTMD", "TopDown Post-GPTMD Proteoforms", "TDGPTMD"),
        ],
        [
            ("Initial", "TopDownInitialSearchProteinGroups", "TDSearch"),
            ("Post-Calib", "TopDownPostCalibrationSearchProteinGroups", "TDCalibrate"),
            ("Post-Avg", "TopDownPostAveragingSearchProteinGroups", "TDAverage"),
            ("Post-GPTMD", "TopDownPostGPTMDSearchProteinGroups", "TDGPTMD"),
        ],
    ],
    # Other Search
    [
        [
            ("Semi-Specific", "Semispecific PSMs", "Semi-Specific"),
            ("Non-Specific", "Nonspecific PSMs", "Non-Specific"),
            ("Modern", "Modern Search PSMs", "Modern"),
            ("Glyco", "Glyco Search PSMs", "Glyco"),
            ("Interlink", "Interlink CSMs", "Interlink"),
            ("Intralink", "Intralink CSMs", "Intralink"),
            ("Loop", "Loop CSMs", "Loop"),
            ("Single", "Crosslink Single PSMs", "Crosslink Single"),
            ("Deadend", "Deadend CSMs", "Deadend"),
        ],
        TIME_ROW_DEFS[2][1],
        [
            ("Semi-Specific", "Semispecific Peptides", "Semi-Specific"),
            ("Non-Specific", "Nonspecific Peptides", "Non-Specific"),
            ("Modern", "Modern Search Peptides", "Modern"),
        ],
        [
            ("Semi-Specific", "SemiSpecificProteinGroups", "Semi-Specific"),
            ("Non-Specific", "NonSpecificProteinGroups", "Non-Specific"),
            ("Modern", "ModernSearchProteinGroups", "Modern"),
            ("Glyco", "GlycoSearchProteinGroups", "Glyco"),
        ],
    ],
]


def draw_lines(
    ax,
    df,
    entries,
    color_dict,
    show_x,
):
    dates = df[plot_common.DATE_COL]
    values = {column: pandas_numeric(df[column]) for _, column, _ in entries}
    vmax = max(v.max() for v in values.values())
    vmin = min(v.min() for v in values.values())
    tight = len(entries) >= 2 and vmax > 0 and (vmax - vmin) / vmax < 0.02

    for k, (label, column, color_label) in enumerate(entries):
        color = color_dict[color_label]
        series = values[column]
        ax.plot(
            dates,
            series,
            marker="o",
            linestyle="-",
            markersize=3.5,
            linewidth=1.5,
            color=color,
            label=label,
        )
        for d, value in zip(dates, series):
            if pandas.notna(value):
                ax.annotate(
                    plot_common.full_number(value),
                    (d, value),
                    textcoords="offset points",
                    xytext=(0, 6),
                    ha="center",
                    fontsize=8,
                    fontweight="bold",
                    color=color,
                )

    if tight:
        low = vmin - 1.0
        high = vmax + 1.0
        ax.set_ylim(low, high)
        ax.set_yticks(numpy.linspace(low, high, 4))
    else:
        ymin, ymax = ax.get_ylim()
        if ymin > 0:
            ax.set_ylim(ymin * 0.9, ymax * 1.12)
    ax.grid(True, alpha=0.3, linewidth=0.5)
    date_span = dates.max() - dates.min()
    if date_span.total_seconds() > 0:
        first_date = mdates.date2num(dates.min().to_pydatetime())
        last_date = mdates.date2num(dates.max().to_pydatetime())
        span_days = date_span.total_seconds() / 86400.0
        ax.set_xlim(first_date - span_days * 0.08, last_date + span_days * 0.25)

    endpoint_values = []
    for k, (label, column, color_label) in enumerate(entries):
        series = values[column]
        last = series.last_valid_index()
        if last is not None:
            endpoint_values.append(
                (k, label, last, series.iloc[last], color_dict[color_label])
            )

    ymin, ymax = ax.get_ylim()
    yrange = ymax - ymin
    sorted_endpoints = sorted(endpoint_values, key=lambda item: item[3])
    offsets = {item[0]: 0 for item in endpoint_values}
    cluster = []
    for endpoint in sorted_endpoints:
        if cluster and (endpoint[3] - cluster[-1][3]) / yrange >= 0.055:
            assign_endpoint_offsets(cluster, offsets)
            cluster = []
        cluster.append(endpoint)
    assign_endpoint_offsets(cluster, offsets)

    for k, label, last, value, color in endpoint_values:
        ax.annotate(
            label,
            (dates.iloc[last], value),
            textcoords="offset points",
            xytext=(8, offsets[k]),
            ha="left",
            va="center",
            fontsize=10,
            fontweight="bold",
            color=color,
            clip_on=False,
        )

    if not show_x:
        plt.setp(ax.get_xticklabels(), visible=False)


def render_stacked(ax, df, entries, color_dict):
    dates = df[plot_common.DATE_COL]
    series = [df[col].fillna(0.0).values for _, col, _ in entries]
    colors = [color_dict[c] for _, _, c in entries]
    labels = [label for label, _, _ in entries]
    totals = sum(series)

    ax.stackplot(dates, series, colors=colors, alpha=0.75, linewidth=0.0)
    ax.plot(dates, totals, color="black", linewidth=1.0, linestyle="--")

    for i, total in enumerate(totals):
        ax.annotate(
            plot_common.compact_number(total),
            (dates[i], total),
            textcoords="offset points",
            xytext=(0, 3),
            ha="center",
            fontsize=10,
            fontweight="bold",
        )

    ax.set_ylim(0, max(totals) * 1.16)
    ax.grid(True, alpha=0.25, linewidth=0.5, axis="y")
    short_labels = {
        "Initial Search": "Initial",
        "Calibration": "Calib",
        "Post-Calib Search": "Post-Calib",
        "Averaging": "Avg",
        "Post-Avg Search": "Post-Avg",
        "Post-GPTMD Search": "Post-GPTMD",
        "Cross-Link": "XL",
    }
    handles = [plt.Rectangle((0, 0), 1, 1, color=color) for color in colors]
    display_labels = [short_labels.get(label, label) for label in labels]
    ax.legend(
        handles[::-1],
        display_labels[::-1],
        loc="center left",
        bbox_to_anchor=(1.02, 0.5),
        ncol=1,
        fontsize=7,
        framealpha=0.9,
        borderaxespad=0,
        handlelength=1.2,
    )


def render_cell(ax, df, entries, color_dict, mode):
    entries = plot_common.available(df, entries)
    if not entries:
        ax.set_visible(False)
        return

    if mode == "lines":
        draw_lines(ax, df, entries, color_dict, show_x=True)
    else:
        render_stacked(ax, df, entries, color_dict)

    for label in ax.get_yticklabels():
        label.set_fontsize(9)


def render_ou_psm_split(fig, ax, df, entries, color_dict):
    pos = ax.get_position()
    ax.set_visible(False)
    gap = 0.035
    half = (pos.height - gap) / 2.0
    top = fig.add_axes([pos.x0, pos.y0 + half + gap, pos.width, half])
    bot = fig.add_axes([pos.x0, pos.y0, pos.width, half])

    main = [e for e in entries if e[0] in ("Semi-Specific", "Non-Specific", "Modern")]
    small = [e for e in entries if e not in main]

    draw_lines(top, df, main, color_dict, show_x=False)
    draw_lines(bot, df, small, color_dict, show_x=True)

    plot_common.configure_date_axis(bot, df[plot_common.DATE_COL])
    for tick in bot.get_xticklabels():
        tick.set_fontsize(8)
    for label in top.get_yticklabels() + bot.get_yticklabels():
        label.set_fontsize(8)


def main():
    df = plot_common.load_data()
    color_dict = plot_common.load_colors()

    fig = plt.figure(figsize=(15.5, 10.5))
    gs = GridSpec(
        3,
        4,
        figure=fig,
        left=0.07,
        right=0.96,
        top=0.90,
        bottom=0.10,
        hspace=0.16,
        wspace=0.34,
    )

    for row in range(3):
        row_pos = None
        for col in range(4):
            ax = fig.add_subplot(gs[row, col])
            if col == 1:
                pos = ax.get_position()
                ax.set_position([pos.x0, pos.y0, pos.width * 0.84, pos.height])
            if (row, col) == (2, 0):
                render_ou_psm_split(fig, ax, df, BOXES[row][col], color_dict)
            else:
                mode = "stacked" if col == 1 else "lines"
                render_cell(ax, df, BOXES[row][col], color_dict, mode)
                if BOXES[row][col]:
                    plot_common.configure_date_axis(ax, df[plot_common.DATE_COL])
                    for tick in ax.get_xticklabels():
                        tick.set_fontsize(8)
            if row_pos is None:
                pos = ax.get_position()
                row_pos = pos.y0 + pos.height / 2.0
        fig.text(
            0.02,
            row_pos,
            ROW_NAMES[row],
            va="center",
            ha="center",
            rotation=90,
            fontsize=14,
            fontweight="bold",
        )

    for col, name in enumerate(COLUMN_NAMES):
        ax = fig.get_axes()[col]
        pos = ax.get_position()
        fig.text(
            pos.x0 + pos.width / 2.0,
            0.965,
            name,
            ha="center",
            va="top",
            fontsize=15,
            fontweight="bold",
        )

    out_path = os.path.join(str(sys.argv[1]), "MasterGrid.png")
    try:
        fig.savefig(out_path, dpi=150)
    except OSError:
        out_path = os.path.join(str(sys.argv[1]), "MasterGrid_new.png")
        fig.savefig(out_path, dpi=150)
    print("wrote", out_path)


if __name__ == "__main__":
    main()
