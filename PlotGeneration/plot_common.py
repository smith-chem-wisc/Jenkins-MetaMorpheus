import os
import sys

import matplotlib.dates as mdates
import pandas

SCRIPT_DIR = os.path.dirname(os.path.realpath(__file__))
DATE_COL = "Date"


def load_data():
    directory = str(sys.argv[1])
    df = pandas.read_csv(os.path.join(directory, "ProcessedResults.csv"))
    df[DATE_COL] = pandas.to_datetime(df[DATE_COL])
    return df


def load_colors():
    colors = pandas.read_csv(os.path.join(SCRIPT_DIR, "PlotColorDict.csv"))
    return dict(zip(colors["Label"], colors["Color"]))


def available(df, entries):
    """Filter (label, column, color_label) tuples to those with at least one numeric value."""
    return [
        (label, col, color_label)
        for label, col, color_label in entries
        if col in df.columns
        and pandas.to_numeric(df[col], errors="coerce").notna().any()
    ]


def configure_date_axis(ax, dates, max_ticks=12):
    """Explicit date ticks for small run counts; AutoDateLocator for long histories."""
    num_runs = len(dates)
    date_numbers = [
        mdates.date2num(
            value.to_pydatetime() if hasattr(value, "to_pydatetime") else value
        )
        for value in dates
    ]
    if num_runs <= max_ticks:
        ax.set_xticks(date_numbers)
    else:
        ax.xaxis.set_major_locator(mdates.AutoDateLocator(maxticks=max_ticks))
    ax.xaxis.set_major_formatter(mdates.DateFormatter("%m/%d"))
    rotation = 0 if num_runs <= 8 else 30
    for tick in ax.get_xticklabels():
        tick.set_rotation(rotation)
        tick.set_fontsize(8)


def full_number(value):
    """Entire number with thousands separators (e.g. '30,653')."""
    try:
        value = float(value)
    except (TypeError, ValueError):
        return "\u2013"
    if value.is_integer():
        return "{:,}".format(int(value))
    return "{:,}".format(value)


def compact_number(value, is_time=False):
    """Short readable cell label: times 'HH.M' / 'M.MM', counts shortened with k suffix."""
    if value is None:
        return "\u2013"
    try:
        value = float(value)
    except (TypeError, ValueError):
        return "\u2013"
    if is_time:
        return "%.1f" % value
    if value >= 1e6:
        return "%.1fM" % (value / 1e6)
    if value >= 1e4:
        return "%.0fk" % (value / 1e3)
    if value >= 1e3:
        return "%.1fk" % (value / 1e3)
    return "%.0f" % value
