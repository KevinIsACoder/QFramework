local BilliardsAudio = {}

local path = "Game/Billiards/Audio/"

function BilliardsAudio.PlaySound(name, ...)
    AudioManager.PlaySound(path..name,...)
end

function BilliardsAudio.PlayBackground(name, ...)
    AudioManager.PlayBackground(path..name,...)
end

return BilliardsAudio