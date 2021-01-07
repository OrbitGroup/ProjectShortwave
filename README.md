# ProjectShortwave
Latency Arbitrage Scanner

This project is named after the shortwave radio, which is capable of sending information (of very low bandwidth) over staggering distances with extremely low latency. Hedge funds
use shortwave radios today to identify and execute trades for identical markets between exchanges in different parts of the world (ie Chicago and London) with the lowest possible
latency. The latency advantage is so profound that the transmission of information is rumored to be seconds faster compared to conventional methods of communicating over large
distances (which is significant in a field otherwise dominated by nanoseconds). Unfortunately, there is little public/verified information about latency arbitrage.

The hypothesis of this strategy is that in the case of identical markets being traded on multiple exchanges, the exchange with greater volume will "lead" the order flow of the
exchange with lesser volume, creating a potential trading opportunity. So, the strategy is not directly based on having the lowest latency over large distances, but is instead
meant to capitalize on smaller exchanges having less active volume to 'react' quickly to price-action in a larger market.

Currently the application cross-references all BTC-quoted markets for Binance and Bitrue, and finds the markets with the most disproportionate volumes (based on the volume traded 
in the last 24 hours). In using the application so far, I have seen markets with 24 hour volumes that are up to 300x higher on Binance compared to Bitrue.

Once the five most disproportionate markets are identified, websocket subscriptions are established with Binance to stream trades executed in real time. Bitrue does not offer
websockets, however its REST API allows up to 41.6 requests per second, which in this case allows us to pull the most recent price information 8.3 times per second for each market.
With that in mind, the 'stream' of data for Bitrue is not a websocket but a loop which sends a request to Bitrue as often as mechanically possible.

Once streams of data are established the application takes a snapshot every 0.5 seconds and calculates the correlation between the market prices and prints it in a column-delineated
format, which can be easily exported to Excel or PowerBI and charted over time.

This project is in its early stages but is intended to support simulated trading (based on real liquidity available in the market) and also more advanced logging/analytics.
