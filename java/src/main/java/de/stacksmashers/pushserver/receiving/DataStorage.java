package de.stacksmashers.pushserver.receiving;

/**
 * unused
 */
public interface DataStorage {


    /**
     * Add a Message to the Store
     *
     * @param string the message to be stored
     */
    void add(String string);


    /**
     * This methods returns the next message that was received or null, if there are no more messages
     *
     * @return the next message that was received or null, if there are no more messages
     */
    String get();


}
