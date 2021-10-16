local socket = require("socket")
local lanes = require("lanes")

local host, port = "localhost", 8080
local client = assert(socket.tcp())
local status = "not connected"
local processing_input = false


-- connect to hook
local function connect()
  client:connect(host, port);
end


-- check for console input in seperate thread to avoid interferance w/ sockets
local checkInput = lanes.gen(
  "io",
  function()
    local input = io.read("*l")
    return input
  end
)
local input_thread = checkInput()


-- cui startup text
io.write("=== Birds-Eye ===\n")
io.write("> ")

-- main loop
while true do
  -- check for user input is ready to be processed
  if input_thread.status == "done" then
    -- get input from thread
    local input = input_thread[1]

    --[[
      === Commands List ===
      connect:  connect to hook
      get ip:   get socket ip
      get port: get socket port
      help:     display commands list
      quit:     disconnect socket and terminate process
    ]]--
    if input == "connect" then
      if status ~= "connected" then
        connect()
        status = "connecting"
        processing_input = true

      else
        io.write("Already connected to hook\n")
      end

    elseif input == "get ip" then
      io.write(host)

    elseif input == "get port" then
      io.write(port)

    elseif input == "help" then
      io.write(
[[
=== Commands List ===
connect:  connect to hook
get ip:   get socket ip
get port: get socket port
help:     display commands list
quit:     disconnect socket and terminate process
]]
      )

    elseif input == "quit" then
      break

    else
      io.write("Unknown command \""..input.."\": type \"help\" for a list of commands\n")
    end

    -- regenerate thread to check for input again
    input_thread = checkInput()

    -- start new line
    if not processing_input then
      io.write("> ")
    end
  end

  -- say hello to hook!
  if status == "connecting" then
    client:send("hello!\n")
    status = "connected"
  end

  if status == "connected" then
    -- receive message from hook
    client:settimeout(1)
    local hook_msg, socket_err = client:receive()

    if hook_msg ~= nil then
      io.write("HOOK: ", hook_msg, "\n")
      io.write("> ")
      processing_input = false
    end

    -- request emulator state
    client:send("state?\n")
  end
end

-- close client when finished
if client then client:close() end
