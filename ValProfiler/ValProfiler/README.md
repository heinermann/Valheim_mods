## Valheim Profiler

This creates periodic flattened stacktrace samples. It does so by

1. Using Harmony to hook as many functions as possible.
2. Uses a StopWatch to obtain stack frames at semi-regular intervals (50ms).
3. Uses another StopWatch to dump the results to file every 30s.

Output files are currently saved to the mod directly (to be moved later) in the form `YYYY-MM-DD_hh-mm_<threadname>_profile.folded` using [Brendan Gregg's collapsed stack format](https://github.com/jlfwong/speedscope/wiki/Importing-from-custom-sources#brendan-greggs-collapsed-stack-format).

These files can be viewed with the following tools:
- [brendangregg/FlameGraph](https://github.com/brendangregg/FlameGraph)
- [speedscope](https://www.speedscope.app/) (Online)
