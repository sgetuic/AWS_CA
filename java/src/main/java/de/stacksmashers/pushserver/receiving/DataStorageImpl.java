package de.stacksmashers.pushserver.receiving;

import java.util.LinkedList;

/**
 * unused
 */

public class DataStorageImpl implements DataStorage {

    private final LinkedList<String> fifoList = new LinkedList<>();
    private static final DataStorageImpl instance = new DataStorageImpl();


    private DataStorageImpl() {
        //prohibit initiating!
    }

    /**
     * Getter for Singleton Instance
     *
     * @return the Singleton for this class
     */
    public static DataStorageImpl getInstance() {
        return instance;
    }

    @Override
    public synchronized void add(String string) {
        fifoList.add(string);
    }

    @Override
    public synchronized String get() {
        if (!fifoList.isEmpty()) {
            return fifoList.removeFirst();
        } else {
            return null;
        }
    }
}
