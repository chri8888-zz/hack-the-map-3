package com.festiflo.festiflow_minion;

public class Stopwatch {

    /**
     * The time at the last split
     */
    private long mTimeOffset = 0;

    public
    Stopwatch() {
        splitTime();
    }

    /**
     * Split the time resetting the stopwatch to 0
     */
    public
    void splitTime() {
        mTimeOffset = System.currentTimeMillis();
    }

    /**
     * Get the current elapsed milliseconds from the last split
     *
     * @return
     */
    public
    long getMS() {
        return System.currentTimeMillis() - mTimeOffset;
    }

    /**
     * Add an amount to the timeoffset
     *
     * @param amount
     */
    public
    void add(final long amount) {
        mTimeOffset -= amount;
    }
}
