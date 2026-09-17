import os
import sys

import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import pandas

import plot_common

ROW_DEFS = [
    (
        "Bottom-Up",
        [
            ("Initial Search", "Initial Search Time", "Search"),
            ("Calibration", "Calibration Time", "Calibrate"),
            ("Post-Calib Search", "Post-calibration Search Time", "Search"),
            ("GPTMD", "GPTMD Time", "GPTMD"),
            ("Post-GPTMD Search", "Post-GPTMD Search Time", "Search"),
        ],
    ),
    (
        "Top-Down",
        [
            ("Initial Search", "TopDown Initial Search Time", "TDSearch"),
            ("Calibration", "TopDown Calibration Time", "TDCalibrate"),
            ("Post-Calib Search", "TopDown Post-calibration Search Time", "TDSearch"),
            ("Averaging", "TopDown Averaging Time", "TDAverage"),
            ("Post-Avg Search", "TopDown Post-averaging Search Time", "TDSearch"),
            ("GPTMD", "TopDown GPTMD Time", "TDGPTMD"),
            ("Post-GPTMD Search", "TopDown Post-GPTMD Search Time", "TDSearch"),
        ],
    ),
    (
        "Other Search",
        [
            ("Semi-Specific", "Semispecific Search Time", "Semi-Specific"),
            ("Non-Specific", "Nonspecific Search Time", "Non-Specific"),
            ("Cross-Link", "XL Search Time", "XL"),
            ("Modern", "Modern Search Time", "Modern"),
            ("Glyco", "Glyco Search Time", "Glyco"),
        ],
    ),
]


def main():
    df = plot_common.load_data()
    color_dict = plot_common.load_colors()

    fig, axes = plt.subplots(3, 1, figsize=(6.6, 10.5))
    fig.subplots_adjust(top=0.93, bottom=0.08, left=0.16, right=0.60, hspace=0.375)

    for ax, (title, entries) in zip(axes, ROW_DEFS):
        entries = plot_common.available(df, entries)
        if not entries:
            ax.set_visible(False)
            continue

        dates = df[plot_common.DATE_COL]
        series = [df[col].fillna(0.0).values for _, col, _ in entries]
        colors = [color_dict[c] for _, _, c in entries]
        labels = [label for label, _, _ in entries]
        totals = sum(series)

        ax.stackplot(
            dates, series, labels=labels, colors=colors, alpha=0.75, linewidth=0.0
        )
        ax.plot(dates, totals, color="black", linewidth=1.2, linestyle="--")

        for i, total in enumerate(totals):
            ax.annotate(
                "%.0f" % total,
                (dates[i], total),
                textcoords="offset points",
                xytext=(0, 4),
                ha="center",
                fontsize=12,
                fontweight="bold",
            )

        ax.set_title(title, fontsize=16, fontweight="bold")
        ax.set_ylabel("Time (min)", fontsize=13)
        ax.set_ylim(0, max(totals) * 1.18)
        ax.grid(True, alpha=0.25, linewidth=0.5, axis="y")
        plot_common.configure_date_axis(ax, dates)
        for tick in ax.get_xticklabels():
            tick.set_fontsize(12)

        handles = [
            plt.Rectangle((0, 0), 1, 1, color=color_dict[color_label])
            for _, _, color_label in entries
        ]
        legend_labels = [label for label, _, _ in entries]
        handles.append(
            plt.Line2D([0], [0], color="black", linestyle="--", linewidth=1.2)
        )
        legend_labels.append("Total")
        ax.legend(
            handles,
            legend_labels,
            loc="center left",
            bbox_to_anchor=(1.05, 0.5),
            ncol=1,
            fontsize=12,
            framealpha=0.9,
            borderaxespad=0,
            handlelength=1.5,
        )

    fig.suptitle("Task Run Time", fontsize=18, fontweight="bold")
    out_path = os.path.join(str(sys.argv[1]), "TaskTimeOverview.png")
    try:
        fig.savefig(out_path, dpi=150)
    except OSError:
        out_path = os.path.join(str(sys.argv[1]), "TaskTimeOverview_new.png")
        fig.savefig(out_path, dpi=150)
    print("wrote", out_path)


if __name__ == "__main__":
    main()
