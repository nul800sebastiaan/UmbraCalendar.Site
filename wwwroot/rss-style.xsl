<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ev="http://purl.org/rss/1.0/modules/event/">
  <xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes"/>
  <xsl:template match="/">
    <html xmlns="http://www.w3.org/1999/xhtml" lang="en">
      <head>
        <meta charset="UTF-8"/>
        <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
        <title><xsl:value-of select="/rss/channel/title"/> - RSS Feed</title>
        <style type="text/css">
          * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
          }
          body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333;
            background: #f5f5f5;
            padding: 20px;
          }
          .container {
            max-width: 900px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
          }
          .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            border-radius: 8px 8px 0 0;
          }
          .header h1 {
            font-size: 2.5em;
            margin-bottom: 10px;
          }
          .header p {
            font-size: 1.1em;
            opacity: 0.9;
          }
          .info {
            background: #f8f9fa;
            padding: 20px 40px;
            border-bottom: 1px solid #e0e0e0;
          }
          .info p {
            margin: 5px 0;
            font-size: 0.95em;
            color: #666;
          }
          .info a {
            color: #667eea;
            text-decoration: none;
          }
          .info a:hover {
            text-decoration: underline;
          }
          .subscribe-info {
            background: #e8f4f8;
            padding: 15px 40px;
            border-left: 4px solid #667eea;
            margin: 0;
          }
          .subscribe-info p {
            color: #555;
            font-size: 0.9em;
          }
          .subscribe-info code {
            background: white;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 0.85em;
            color: #d63384;
          }
          .items {
            padding: 0 40px 40px;
          }
          .item {
            padding: 30px 0;
            border-bottom: 1px solid #e0e0e0;
          }
          .item:last-child {
            border-bottom: none;
          }
          .item h2 {
            margin-bottom: 10px;
          }
          .item h2 a {
            color: #333;
            text-decoration: none;
            font-size: 1.5em;
          }
          .item h2 a:hover {
            color: #667eea;
          }
          .item-meta {
            color: #666;
            font-size: 0.9em;
            margin-bottom: 10px;
          }
          .item-meta span {
            margin-right: 15px;
          }
          .item-meta strong {
            color: #333;
          }
          .item-description {
            color: #555;
            line-height: 1.7;
            margin-top: 10px;
          }
          .event-details {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-top: 10px;
            font-size: 0.9em;
          }
          .event-details div {
            margin: 5px 0;
          }
          .event-details strong {
            color: #667eea;
            margin-right: 5px;
          }
          .footer {
            text-align: center;
            padding: 20px;
            color: #999;
            font-size: 0.85em;
            border-top: 1px solid #e0e0e0;
          }
        </style>
      </head>
      <body>
        <div class="container">
          <div class="header">
            <h1><xsl:value-of select="/rss/channel/title"/></h1>
            <p><xsl:value-of select="/rss/channel/description"/></p>
          </div>

          <div class="info">
            <p>
              <strong>Feed URL:</strong>
              <a href="{/rss/channel/link}"><xsl:value-of select="/rss/channel/link"/></a>
            </p>
            <p><strong>Last Updated:</strong> <xsl:value-of select="/rss/channel/lastBuildDate"/></p>
          </div>

          <div class="subscribe-info">
            <p>
              📡 <strong>Subscribe to this feed</strong> - Copy the current page URL and paste it into your RSS reader.
              This is an RSS feed. Use an RSS reader to subscribe to updates.
            </p>
          </div>

          <div class="items">
            <xsl:for-each select="/rss/channel/item">
              <div class="item">
                <h2>
                  <a href="{link}" target="_blank">
                    <xsl:value-of select="title"/>
                  </a>
                </h2>

                <div class="item-meta">
                  <span>📅 <strong>Published:</strong> <xsl:value-of select="pubDate"/></span>
                </div>

                <div class="item-description">
                  <xsl:value-of select="description"/>
                </div>

                <xsl:if test="ev:startdate or ev:location or ev:organizer">
                  <div class="event-details">
                    <xsl:if test="ev:startdate">
                      <div>
                        <strong>🕐 Start:</strong> <xsl:value-of select="ev:startdate"/>
                      </div>
                    </xsl:if>
                    <xsl:if test="ev:enddate">
                      <div>
                        <strong>🕐 End:</strong> <xsl:value-of select="ev:enddate"/>
                      </div>
                    </xsl:if>
                    <xsl:if test="ev:location">
                      <div>
                        <strong>📍 Location:</strong> <xsl:value-of select="ev:location"/>
                      </div>
                    </xsl:if>
                    <xsl:if test="ev:organizer">
                      <div>
                        <strong>👥 Organizer:</strong> <xsl:value-of select="ev:organizer"/>
                      </div>
                    </xsl:if>
                  </div>
                </xsl:if>
              </div>
            </xsl:for-each>
          </div>

          <div class="footer">
            <p><xsl:value-of select="/rss/channel/copyright"/></p>
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
